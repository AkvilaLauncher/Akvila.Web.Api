using Akvila.Web.Api.Dto.Integration;
using FluentValidation;
using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Core.Validation;

public class IntegrationValidator : AbstractValidator<IntegrationUpdateDto> {
    public IntegrationValidator() {
        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required.")
            .Must((dto, endpoint) => {
                if (dto.AuthType == AuthType.Microsoft) {
                    return Guid.TryParse(endpoint, out _);
                }
                return IsValidUrl(endpoint);
            }).WithMessage(x => x.AuthType == AuthType.Microsoft
                ? "Endpoint must be a valid Application ID."
                : "Endpoint must be a valid URL.");
    }

    private bool IsValidUrl(string url) {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}