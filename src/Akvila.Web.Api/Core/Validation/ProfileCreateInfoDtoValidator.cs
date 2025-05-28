using Akvila.Web.Api.Dto.Profile;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class ProfileCreateInfoDtoValidator : AbstractValidator<ProfileCreateInfoDto> {
    public ProfileCreateInfoDtoValidator() {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 100).WithMessage("The length of the username should be between 2 and 100 characters long.");
        RuleFor(x => x.UserAccessToken)
            .NotEmpty().WithMessage("User token is required.")
            .Length(2, 1000).WithMessage("The length of the user token should be between 2 and 1000 characters long.");
        RuleFor(x => x.ProfileName)
            .NotEmpty().WithMessage("Profile name is required.")
            .Length(2, 100).WithMessage("The length of the client's name should be between 2 and 100 characters long.");
        RuleFor(x => x.UserUuid)
            .NotEmpty().WithMessage("UserUUID is required.")
            .Length(2, 100).WithMessage("The length of the UserUUID must be between 2 and 100 characters long.");
        RuleFor(x => x.OsArchitecture)
            .NotEmpty().WithMessage("OS architecture is required.")
            .Length(2, 100)
            .WithMessage("The length of the OS architecture should be between 2 and 100 characters long.");
        RuleFor(x => x.OsType);
    }
}