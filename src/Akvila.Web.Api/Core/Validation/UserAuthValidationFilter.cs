using Akvila.Web.Api.Dto.User;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class UserAuthValidationFilter : AbstractValidator<UserAuthDto> {
    public UserAuthValidationFilter() {
        RuleFor(c => c.Login)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Please enter your login.")
            .Length(3, 50).WithMessage("Login must be between 3 and 50 characters.")
            .Matches("^[a-zA-Z0-9]*$").WithMessage("Login must consist of letters and numbers only.");

        RuleFor(c => c.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Please enter your password.")
            .Length(8, 100).WithMessage("Password must be between 8 and 100 characters.")
            .Must(ContainAtLeastOneUppercaseLetter).WithMessage("The password must contain at least one capital letter.")
            .Must(ContainAtLeastOneLowercaseLetter).WithMessage("The password must contain at least one lowercase letter.")
            .Must(ContainAtLeastOneNumber).WithMessage("The password must contain at least one digit.");
    }

    private bool ContainAtLeastOneUppercaseLetter(string password) {
        return password.Any(char.IsUpper);
    }

    private bool ContainAtLeastOneLowercaseLetter(string password) {
        return password.Any(char.IsLower);
    }

    private bool ContainAtLeastOneNumber(string password) {
        return password.Any(char.IsDigit);
    }
}