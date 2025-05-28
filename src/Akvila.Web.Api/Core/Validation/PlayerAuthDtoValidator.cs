using Akvila.Web.Api.Dto.User;
using FluentValidation;
using Spectre.Console;

namespace Akvila.Web.Api.Core.Validation;

public class PlayerAuthDtoValidator : AbstractValidator<BaseUserPassword> {
    public PlayerAuthDtoValidator() {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("The login field is required.")
            .Length(3, 50).WithMessage("Login must contain from 3 to 50 characters.");
    }
}