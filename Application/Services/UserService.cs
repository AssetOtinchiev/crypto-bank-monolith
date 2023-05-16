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
    public UserService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<UserDto, ErrorResponse>> Register(CreateUserDto userDto, CancellationToken cancellationToken)
    {
        //todo fluentValidator
        
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
    
}