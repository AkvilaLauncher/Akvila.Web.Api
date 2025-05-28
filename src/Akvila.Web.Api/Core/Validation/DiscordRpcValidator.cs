using Akvila.Web.Api.Dto.Integration;
using FluentValidation;
using Akvila.Web.Api.Dto.Profile;

namespace Akvila.Web.Api.Core.Validation;

public class DiscordRpcValidator : AbstractValidator<DiscordRpcUpdateDto> {
    public DiscordRpcValidator() {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required")
            .Length(1, 32).WithMessage("ClientId must contain from 1 to 32 characters");

        RuleFor(x => x.Details)
            .MaximumLength(128).WithMessage("Details cannot be longer than 128 characters");

        RuleFor(x => x.LargeImageKey)
            .MaximumLength(32).WithMessage("LargeImageKey cannot be longer than 32 characters");

        RuleFor(x => x.LargeImageText)
            .MaximumLength(128).WithMessage("LargeImageText cannot be longer than 128 characters");

        RuleFor(x => x.SmallImageKey)
            .MaximumLength(32).WithMessage("SmallImageKey cannot be longer than 32 characters");

        RuleFor(x => x.SmallImageText)
            .MaximumLength(128).WithMessage("SmallImageText cannot be longer than 128 characters");
    }
}