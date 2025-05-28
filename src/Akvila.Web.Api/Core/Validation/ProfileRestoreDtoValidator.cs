using Akvila.Web.Api.Dto.Profile;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class ProfileRestoreDtoValidator : AbstractValidator<ProfileRestoreDto> {
    public ProfileRestoreDtoValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Matches("^[a-zA-Z0-9-]*$")
            .WithMessage("The profile name can only contain English letters, numbers and dashes.")
            .Length(2, 100).WithMessage("The length of the name should be between 2 and 100 characters.");
    }
}