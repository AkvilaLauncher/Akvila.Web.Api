using Akvila.Web.Api.Dto.Texture;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class TextureServiceDtoValidator : AbstractValidator<UrlServiceDto> {
    public TextureServiceDtoValidator() {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .Must(ValidateUrl).WithMessage("Endpoint must be a valid URL.");
    }

    private bool ValidateUrl(string url) {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private bool ContainUserName(string? url) {
        return url?.Contains("{userName}") ?? false;
    }
}