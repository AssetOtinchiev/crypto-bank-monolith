using Contracts.Dtos;
using Domain.Entitites;

namespace Application.Mappers;

public static class CustomMapper
{
    public static User ToUser(CreateUserDto createUserDto, string passwordHash, string passwordSalt)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Password = passwordHash,
            PasswordSalt = passwordSalt,
            Email = createUserDto.Email
        };
    }

    public static UserDto ToUserDto(User user)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.DeletedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}