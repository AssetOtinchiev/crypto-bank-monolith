namespace Application.Interfaces;

public interface ITokenService
{
    Task<Tuple<string, string>> GenerateTokensAsync(Guid userId, CancellationToken cancellationToken);
    //Task<bool> RemoveRefreshTokenAsync(User user);
}