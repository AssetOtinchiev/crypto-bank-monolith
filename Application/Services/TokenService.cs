using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class TokenService : ITokenService
{
    private readonly IAppDbContext _dbContext;

    public TokenService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tuple<string, string>> GenerateTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accessToken = await TokenHelper.GenerateAccessToken(userId);
        var refreshToken = await TokenHelper.GenerateRefreshToken();

        var userRecord = await _dbContext.Users
            .Include(o => o.RefreshTokens)
            .FirstOrDefaultAsync(e => e.Id == userId, cancellationToken: cancellationToken);

        if (userRecord == null)
        {
            return null;
        }

        var salt = PasswordHelper.GetSecureSalt();

        var refreshTokenHashed = PasswordHelper.HashUsingPbkdf2(refreshToken, salt);

        if (userRecord.RefreshTokens != null && userRecord.RefreshTokens.Any())
        {
            await RemoveRefreshTokenAsync(userRecord);

        }
        userRecord.RefreshTokens?.Add(new RefreshToken
        {
            ExpiryDate = DateTime.Now.AddDays(14).ToUniversalTime(),
            CreatedAt = DateTime.Now.ToUniversalTime(),
            UserId = userId,
            TokenHash = refreshTokenHashed,
            TokenSalt = Convert.ToBase64String(salt)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = new Tuple<string, string>(accessToken, refreshToken);

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