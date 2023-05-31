using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Users.Domain;

namespace WebApi.Features.Auth.Services;

public class TokenService
{
    private readonly AppDbContext _dbContext;
    private readonly TokenHelper _tokenHelper;

    public TokenService(AppDbContext dbContext, TokenHelper tokenHelper)
    {
        _dbContext = dbContext;
        _tokenHelper = tokenHelper;
    }

    public async Task<(string, string)> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var accessToken = await _tokenHelper.GenerateAccessToken(user);
        
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