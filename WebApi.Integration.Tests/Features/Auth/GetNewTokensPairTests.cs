using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Options;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Auth;

public class GetNewTokensPairTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;
    private JwtOptions _jwtOptions = new();

    public GetNewTokensPairTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_generate_tokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "test@test.com";
        (await client.PostAsJsonAsync("/users", new
            {
                Email = email,
                Password = "qwerty123456A!",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
            }, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = email,
            Password = "qwerty123456A!"
        }, cancellationToken: _cancellationToken);

        response.EnsureSuccessStatusCode();

        var accessTokenModel = await response.Content.ReadFromJsonAsync<AccessTokenModel>(cancellationToken: _cancellationToken);
        accessTokenModel.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();
        
        IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
        client.DefaultRequestHeaders.Add("Set-Cookie", cookies.SingleOrDefault());

        // Act
        var newTokenPairResponse = await client.GetFromJsonAsync<AccessTokenModel>($"/auth/newTokens", cancellationToken: _cancellationToken);

        // Assert
        newTokenPairResponse.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();

        var userId = await GetUserIdFromToken(accessTokenModel.AccessToken);
        var user = await _db.Users.FindAsync(userId);
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
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
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        _jwtOptions = _scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value.Jwt;
        
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.RefreshTokens.RemoveRange(_db.RefreshTokens);
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(_cancellationToken);
        await _scope.DisposeAsync();
    }
}