using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Users.Domain;
using WebApi.Shared;

namespace WebApi.Features.Auth.Services;

public class TokenService
{
    private readonly AppDbContext _dbContext;
    private readonly TokenHelper _tokenHelper;
    private readonly PasswordHelper _passwordHelper;

    public TokenService(AppDbContext dbContext, TokenHelper tokenHelper, PasswordHelper passwordHelper)
    {
        _dbContext = dbContext;
        _tokenHelper = tokenHelper;
        _passwordHelper = passwordHelper;
    }

    public async Task<(string, string)> GenerateTokensAsync(User user, string deviceName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshTokens = user.RefreshTokens
            .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.DeviceName == deviceName)
            .ToArray();

        var expiredTokens = refreshTokens
            .Where(x => x.ExpiryDate <= DateTime.Now.ToUniversalTime()).ToArray();

        var activeRefreshToken = refreshTokens.FirstOrDefault(x => !x.IsRevorked);

        var accessToken = await _tokenHelper.GenerateAccessToken(user);
        var refreshToken = await _tokenHelper.GenerateRefreshToken();

        var salt = _passwordHelper.GetSecureSalt();
        var refreshTokenHashed = _passwordHelper.GetHashUsingArgon2(refreshToken, salt);
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var newRefreshToken = new RefreshToken
            {
                ExpiryDate = DateTime.Now.AddDays(14).ToUniversalTime(),
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UserId = user.Id,
                TokenHash = refreshTokenHashed,
                TokenSalt = Convert.ToBase64String(salt),
                DeviceName = deviceName
            };
            user.RefreshTokens?.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (activeRefreshToken != null)
            {
                activeRefreshToken.IsRevorked = true;
                activeRefreshToken.ReplacedBy = newRefreshToken.Id;
            }

            _dbContext.RefreshTokens.RemoveRange(expiredTokens);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        (string, string) token = (accessToken, refreshToken);

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