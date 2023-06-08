using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Database;
using WebApi.Features.Auth.Models;

namespace WebApi.Integration.Tests.Features.Auth;

public class GetNewTokensPair : IClassFixture<TestingWebAppFactory<Program>>, IAsyncLifetime
{
    private readonly TestingWebAppFactory<Program> _factory;
    private AppDbContext _db;
    private AsyncServiceScope _scope;

    public GetNewTokensPair(TestingWebAppFactory<Program> factory)
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
                DateOfBirth = "2000-01-31",
            }))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/auth", new
        {
            Email = "test@test.com",
            Password = "qwerty123456A!"
        });

        response.EnsureSuccessStatusCode();

        var accessTokenModel = await response.Content.ReadFromJsonAsync<AccessTokenModel>();
        accessTokenModel.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();

        IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
        client.DefaultRequestHeaders.Add("Set-Cookie", cookies.SingleOrDefault());

        // Act
        var newTokenPairResponse = await client.GetFromJsonAsync<AccessTokenModel>($"/auth/newTokens");

        // Assert
        newTokenPairResponse.Should().NotBeNull();
        accessTokenModel.AccessToken.Should().NotBeEmpty();
    }

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        await _scope.DisposeAsync();
    }
}