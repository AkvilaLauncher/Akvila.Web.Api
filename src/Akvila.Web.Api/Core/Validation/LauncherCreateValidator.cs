using Akvila.Web.Api.Dto.Launcher;
using FluentValidation;

namespace Akvila.Web.Api.Core.Validation;

public class LauncherCreateDtoValidator : AbstractValidator<LauncherCreateDto> {
    public LauncherCreateDtoValidator() {
        RuleFor(x => x.GitHubVersions)
            .NotEmpty().WithMessage("The GitHubVersions field is required to be filled in.")
            .Length(3, 50).WithMessage("GitHubVersions must contain between 3 and 50 characters.");

        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("The Host field is required.")
            .Length(3, 50).WithMessage("Host must contain between 3 and 50 characters.");

        RuleFor(x => x.Folder)
            .NotEmpty().WithMessage("The Folder field is required.")
            .Length(3, 50).WithMessage("Folder must contain between 3 and 50 characters.")
            .Matches(@"^[^\\\/:*?""<>|]+$")
            .WithMessage("Folder must not contain invalid characters: \\ / : * ? \" < > |");
    }
}