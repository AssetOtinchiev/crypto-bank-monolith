using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Users.Domain;
using PasswordHelper = WebApi.Shared.PasswordHelper;

namespace WebApi.Features.Auth.Services;

public class TokenService
{
    private readonly AppDbContext _dbContext;

    public TokenService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(string, string)> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var accessToken = await TokenHelper.GenerateAccessToken(user);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        (string, string) token = (accessToken, "refreshToken");

        return token;
    }

    public async Task<bool> RemoveRefreshTokenAsync(User user)
    {
        var userRecord = await _dbContext.Users
            .Include(o => o.RefreshTokens)
            .FirstOrDefaultAsync(e => e.Id == user.Id);

        if (userRecord == null)
        {
            return false;
        }

        if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
        {
            var currentRefreshToken = userRecord.RefreshTokens.First();

            _dbContext.RefreshTokens.Remove(currentRefreshToken);
        }

        return false;
    }
}