using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Claims;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Database;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Options;
using WebApi.Features.Auth.Requests;
using WebApi.Features.Users.Requests;
using WebApi.Integration.Tests.Features.Users.MockData;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Auth;

public class GetNewTokensPairTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private JwtOptions _jwtOptions = new();
    private CookieHelper _cookieHelper;

    public GetNewTokensPairTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_generate_tokens()
    {
        // Arrange
        var client = _factory.CreateClient();

        var password = "qwerty123456A!";
        var email = "test@test.com";
        var userRequest = new RegisterUser.Request(email, password, DateTime.Now.ToUniversalTime());
        var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);

        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = email,
            Password = password
        }, cancellationToken: _cancellationToken);

        response.EnsureSuccessStatusCode();

        var accessTokenModel =
            await response.Content.ReadFromJsonAsync<AccessTokenModel>(cancellationToken: _cancellationToken);
        accessTokenModel.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();

        // Act
        var newTokenPairResponse = await client.GetAsync($"/auth/newTokens", cancellationToken: _cancellationToken);
        var newTokenPair =
            await newTokenPairResponse.Content.ReadFromJsonAsync<AccessTokenModel>(
                cancellationToken: _cancellationToken);

        // Assert
        newTokenPair.Should().NotBeNull();
        newTokenPair.AccessToken.Should().NotBeEmpty();

        var refreshToken = _cookieHelper.GetCookie(newTokenPairResponse);
        refreshToken.Should().NotBeNull();

        var savedRefreshToken = _db.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);
        savedRefreshToken.Should().NotBeNull();
        savedRefreshToken.UserId.Should().Be(createdUser.Id);

        var userId = await GetUserIdFromToken(newTokenPair.AccessToken);
        var user = await _db.Users.FindAsync(userId);
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task Should_validate_old_token()
    {
        // Arrange
        var client = _factory.CreateClient();

        var password = "qwerty123456A!";
        var email = "test@test.com";
        var userRequest = new RegisterUser.Request(email, password, DateTime.Now.ToUniversalTime());
        var createdUser = await CreateUserHelper.CreateUser(userRequest, _scope, _cancellationToken);

        var oldToken = "test";
        createdUser.RefreshTokens.AddRange(new List<RefreshToken>()
        {
            new()
            {
                Token = oldToken,
                DeviceName = "",
                ExpiryDate = DateTime.Now.ToUniversalTime().AddDays(2),
                IsRevoked = true,
                CreatedAt = DateTime.Now.ToUniversalTime()
            },
            new()
            {
                Token = "test1",
                DeviceName = "",
                ExpiryDate = DateTime.Now.ToUniversalTime().AddDays(2),
                IsRevoked = false,
                CreatedAt = DateTime.Now.ToUniversalTime()
            }
        });
        
        await _db.SaveChangesAsync(_cancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/newTokens");
        request.Headers.Add("Cookie", $"refreshToken={oldToken}");
        // Act
        var newTokenPairResponse = await client.SendAsync(request, cancellationToken: _cancellationToken);

        // Assert
        newTokenPairResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var response =
            await newTokenPairResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: _cancellationToken);
        response.Detail.Should().Be("Invalid token");

        var refreshTokens = await _db.RefreshTokens.AsNoTracking()
            .Where(x => x.UserId == createdUser.Id && x.DeviceName == String.Empty)
            .ToListAsync(cancellationToken: _cancellationToken);
        refreshTokens.Select(x => x.IsRevoked).Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public async Task Should_validate_tokens()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var newTokenPairResponse = await client.GetAsync($"/auth/newTokens", cancellationToken: _cancellationToken);

        // Assert
        newTokenPairResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> GetUserIdFromToken(string accessToken)
    {
        var claimsPrincipal = GetPrincipalFromToken(accessToken);

        var userId = claimsPrincipal.Claims.First(x => x.Type == "userid").Value;
        return Guid.Parse(userId);
    }

    private ClaimsPrincipal GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_jwtOptions.Key)),
            };
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                throw new SecurityTokenException("Invalid token passed");
            }

            return principal;
        }
        catch
        {
            throw new AuthenticationException("One or more validation failures have occurred");
        }
    }

    private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
               jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                   StringComparison.InvariantCultureIgnoreCase);
    }

    public Task InitializeAsync()
    {
         BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _jwtOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value.Jwt;
        _cookieHelper = new CookieHelper();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}

public class GetNewTokensPairValidatorTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private GetNewTokensPair.RequestValidator _validator;
    private CancellationToken _cancellationToken;

    public GetNewTokensPairValidatorTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_require_refresh_token(string refreshToken)
    {
        var result = await _validator.TestValidateAsync(new GetNewTokensPair.Request()
        {
            RefreshToken = refreshToken
        }, cancellationToken: _cancellationToken);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorCode("auth_validation_token_required");
    }

    public Task InitializeAsync()
    {
         BaseInitializeHelper.Initialize(_factory, ref _scope, ref _db, ref _cancellationToken);
        _validator = new GetNewTokensPair.RequestValidator();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        BaseInitializeHelper.DisposeDatabase(ref _db);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}