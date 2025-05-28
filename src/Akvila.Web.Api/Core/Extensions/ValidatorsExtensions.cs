using System.Collections.Frozen;
using Akvila.Web.Api.Core.Validation;
using Akvila.Web.Api.Dto.Files;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.Launcher;
using Akvila.Web.Api.Dto.Mods;
using Akvila.Web.Api.Dto.Profile;
using Akvila.Web.Api.Dto.Servers;
using Akvila.Web.Api.Dto.Texture;
using Akvila.Web.Api.Dto.User;
using FluentValidation;

namespace Akvila.Web.Api.Core.Extensions;

public static class ValidatorsExtensions {
    public static IServiceCollection RegisterValidators(this IServiceCollection serviceCollection) {
        serviceCollection
            // Add auth validators
            .AddScoped<IValidator<UserCreateDto>, UserCreateValidationFilter>()
            .AddScoped<IValidator<UserAuthDto>, UserAuthValidationFilter>()

            // Profiles validator
            .AddScoped<IValidator<ProfileCreateDto>, ProfileCreateDtoValidator>()
            .AddScoped<IValidator<ProfileUpdateDto>, ProfileUpdateDtoValidator>()
            .AddScoped<IValidator<ProfileRestoreDto>, ProfileRestoreDtoValidator>()
            .AddScoped<IValidator<ProfileCompileDto>, CompileProfileDtoValidator>()
            .AddScoped<IValidator<ProfileCreateInfoDto>, ProfileCreateInfoDtoValidator>()

            // Players validator
            .AddScoped<IValidator<BaseUserPassword>, PlayerAuthDtoValidator>()

            // Integration validator
            .AddScoped<IValidator<IntegrationUpdateDto>, IntegrationValidator>()

            // Files validator
            .AddScoped<IValidator<List<FileWhiteListDto>>, FileWhiteListValidator>()
            .AddScoped<IValidator<List<FolderWhiteListDto>>, FolderWhiteListValidator>()

            // Launcher validator
            .AddScoped<IValidator<LauncherCreateDto>, LauncherCreateDtoValidator>()
            .AddScoped<IValidator<ModsDetailsInfoDto>, ModsUpdateInfoValidator>()

            // Servers validator
            .AddScoped<IValidator<CreateServerDto>, CreateServerDtoValidator>()

            // Discord validator
            .AddScoped<IValidator<DiscordRpcUpdateDto>, DiscordRpcValidator>()

            // Texture validator
            .AddScoped<IValidator<UrlServiceDto>, TextureServiceDtoValidator>();

        return serviceCollection;
    }
}