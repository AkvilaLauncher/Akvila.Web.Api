using Akvila.Web.Api.Dto.Profile;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class ProfileCreateDtoValidator : AbstractValidator<ProfileCreateDto> {
    public ProfileCreateDtoValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Matches("^[a-zA-Z0-9-]*$")
            .WithMessage("The profile name can only contain English letters, numbers and dashes.")
            .Length(2, 100).WithMessage("The length of the name should be between 2 and 100 characters.");
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Displayed name is required.")
            .Length(2, 100).WithMessage("The length of the name should be between 2 and 100 characters.");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(2, 1000).WithMessage("The length of the description should be between 2 and 1000 characters.");
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("The version is required.")
            .Length(2, 100).WithMessage("The length of the version should be between 2 and 100 characters.");
    }
}