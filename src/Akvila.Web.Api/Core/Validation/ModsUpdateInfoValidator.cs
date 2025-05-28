using Akvila.Web.Api.Dto.Mods;
using FluentValidation;
using Akvila.Web.Api.Dto.User;

namespace Akvila.Web.Api.Core.Validation;

public class ModsUpdateInfoValidator : AbstractValidator<ModsDetailsInfoDto> {
    public ModsUpdateInfoValidator() {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("The Key field must not be empty.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("The Title field must not be empty.")
            .MaximumLength(100).WithMessage("The Title field must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("The Description field must not be empty.");
    }
}