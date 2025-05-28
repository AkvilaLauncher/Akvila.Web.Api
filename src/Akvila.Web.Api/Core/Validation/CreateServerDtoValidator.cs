using Akvila.Web.Api.Dto.Servers;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class CreateServerDtoValidator : AbstractValidator<CreateServerDto>
{
    public CreateServerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Server name required.")
            .Length(3, 100).WithMessage("The server name should be between 3 and 100 characters long.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Server address is mandatory.")
            .Matches(@"^([a-zA-Z0-9\.\-]+)$").WithMessage("The server address can only contain English letters, numbers, dots and hyphens.")
            .Length(5, 255).WithMessage("The length of the server address must be between 5 and 255 characters long.");

        RuleFor(x => x.Port)
            .InclusiveBetween(0, 65535).WithMessage("The port must be in the range of 0 to 65535.");
    }
}