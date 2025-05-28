using Akvila.Web.Api.Dto.Integration;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class IntegrationValidator : AbstractValidator<IntegrationUpdateDto> {
    public IntegrationValidator() {
        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required.")
            .Must(IsValidUrl).WithMessage("Endpoint must be a valid URL.");
    }

    private bool IsValidUrl(string url) {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}