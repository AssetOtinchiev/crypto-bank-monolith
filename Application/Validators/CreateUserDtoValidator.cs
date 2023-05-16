using Contracts.Dtos;
using FluentValidation;

namespace Application.Validators;

public class CreateUserDtoValidator : AbstractValidator < CreateUserDto > {
public CreateUserDtoValidator() {
    RuleFor(x => x.Email).MinimumLength(4)
        .EmailAddress();
    
    RuleFor(x => x.Password).MinimumLength(7);
}
}