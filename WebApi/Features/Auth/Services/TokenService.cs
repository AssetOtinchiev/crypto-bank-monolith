using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Auth.Options;
using WebApi.Features.Users.Domain;

namespace WebApi.Features.Auth.Services;

public class TokenService
{
    private readonly AppDbContext _dbContext;
    private readonly TokenHelper _tokenHelper;
    private readonly AuthOptions _authOptions;

    public TokenService(AppDbContext dbContext, TokenHelper tokenHelper, IOptions<AuthOptions> authOptions)
    {
        _dbContext = dbContext;
        _tokenHelper = tokenHelper;
        _authOptions = authOptions.Value;
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user, string deviceName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var refreshTokens = user.RefreshTokens
            .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.DeviceName == deviceName)
            .ToArray();

        var accessToken = _tokenHelper.GenerateAccessToken(user);
        var refreshToken = await _tokenHelper.GenerateRefreshToken();
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var newRefreshToken = new RefreshToken
            {
                ExpiryDate = DateTime.Now.Add(_authOptions.RefreshTokenExpiration).ToUniversalTime(),
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UserId = user.Id,
                Token = refreshToken,
                DeviceName = deviceName
            };
            user.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        
            var activeRefreshToken = refreshTokens.FirstOrDefault(x => !x.IsRevoked);
            if (activeRefreshToken != null)
            {
                activeRefreshToken.IsRevoked = true;
                activeRefreshToken.ReplacedBy = newRefreshToken.Id;
            }
        
            var expiredTokens = refreshTokens
                .Where(x => x.ExpiryDate <= DateTime.Now.ToUniversalTime()).ToArray();
            _dbContext.RefreshTokens.RemoveRange(expiredTokens);
        
            await _dbContext.SaveChangesAsync(cancellationToken);
        
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        return (accessToken, refreshToken);
    }
}