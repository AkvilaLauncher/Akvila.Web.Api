using System.Collections.Frozen;
using Akvila.Web.Api.Dto.Files;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class FileWhiteListValidator : AbstractValidator<List<FileWhiteListDto>> {
    public FileWhiteListValidator() {
        RuleForEach(x => x).ChildRules(child => {
            child.RuleFor(x => x.ProfileName)
                .NotEmpty().WithMessage("Profile name required.")
                .Matches("^[a-zA-Z0-9-]*$")
                .WithMessage("The profile name can only contain English letters, numbers and dashes.")
                .Length(2, 100).WithMessage("The length of the profile name should be from 2 to 100 characters.");

            child.RuleFor(x => x.Hash)
                .NotEmpty().WithMessage("Hash is required.")
                .Matches("^[a-fA-F0-9]*$")
                .WithMessage("A hash can only contain HEX characters (digits from 0 to 9 and letters from A to F).");
        });
    }
}