using Contracts.Dtos;
using Contracts.Dtos.Responses;
using OneOf;

namespace Application.Interfaces;

public interface IUserService
{
    Task<OneOf<UserDto, ErrorResponse>> Register(CreateUserDto userDto, CancellationToken cancellationToken);
}