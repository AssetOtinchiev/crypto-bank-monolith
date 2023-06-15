using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Integration.Tests.Helpers;

namespace WebApi.Integration.Tests.Features.Auth;

public class GetNewTokensPairTests : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;
    private CancellationToken _cancellationToken;

    public GetNewTokensPairTests(TestingWebAppFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_generate_tokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/users", new
            {
                Email = "test@test.com",
                Password = "qwerty123456A!",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
            }, cancellationToken: _cancellationToken))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = "test@test.com",
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
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _cancellationToken = new CancellationTokenHelper().GetCancellationToken();
        
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