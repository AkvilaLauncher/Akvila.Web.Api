using Akvila.Web.Api.Dto.Files;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class FolderWhiteListValidator : AbstractValidator<List<FolderWhiteListDto>> {
    public FolderWhiteListValidator() {
        RuleForEach(x => x).ChildRules(child => {
            child.RuleFor(x => x.ProfileName)
                .NotEmpty().WithMessage("Profile name required.")
                .Matches("^[a-zA-Z0-9-]*$")
                .WithMessage("The profile name can only contain English letters, numbers and dashes.")
                .Length(2, 100).WithMessage("The length of the profile name should be from 2 to 100 characters.");

            child.RuleFor(x => x.Path)
                .NotEmpty().WithMessage("The path is required.");
        });
    }
}