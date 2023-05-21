using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure;
using Application.Mappers;
using Contracts.Dtos;
using Contracts.Dtos.Responses;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    public UserService(IAppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<OneOf<UserDto, ErrorResponse>> Register(CreateUserDto userDto, CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == userDto.Email);

        if (existingUser != null)
        {
            return new ErrorResponse
            {
                Errors = {"User already exists with the same email"}
            };
        }

        var salt = PasswordHelper.GetSecureSalt();
        var passwordHash = PasswordHelper.HashUsingPbkdf2(userDto.Password, salt);

        var user = CustomMapper.ToUser(userDto, passwordHash, Convert.ToBase64String(salt));
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CustomMapper.ToUserDto(user);
    }
    
    public async Task<OneOf<TokenResponse, ErrorResponse>> LoginAsync(CreateUserDto loginRequest, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.SingleOrDefault(user => user.Email == loginRequest.Email);

        if (user == null)
        {
            return new ErrorResponse
            {
                Errors = {"Email not found"}
            };
        }

        var passwordHash =
            PasswordHelper.HashUsingPbkdf2(loginRequest.Password, Convert.FromBase64String(user.PasswordSalt));

        if (user.Password != passwordHash)
        {
            return new ErrorResponse
            {
                Errors = {"Invalid Password"}
            };
        }

        var token = await Task.Run(() => _tokenService.GenerateTokensAsync(user.Id, cancellationToken));

        return new TokenResponse
        {
            AccessToken = token.Item1,
            RefreshToken = token.Item2,
            UserId = user.Id,
            Email = user.Email
        };
    }
    
}