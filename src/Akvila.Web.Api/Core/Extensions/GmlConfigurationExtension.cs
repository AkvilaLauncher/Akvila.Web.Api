using Akvila.Web.Api.Domains.Launcher;
using AkvilaCore.Interfaces;
using Akvila;
using Akvila.Core.Launcher;

namespace Akvila.Web.Api.Core.Extensions;

public static class GmlConfigurationExtension {
    public static IServiceCollection ConfigureGmlManager(
        this IServiceCollection services,
        string projectName,
        string securityKey,
        string? projectPath,
        string? textureEndpoint) {
        services.AddSingleton<IAkvilaManager>(_ => {
            var settings = new AkvilaSettings(projectName, securityKey, projectPath) {
                TextureServiceEndpoint = textureEndpoint ?? "http://akvila-web-skins:8085"
            };

            var manager = new AkvilaManager(settings);

            manager.RestoreSettings<LauncherVersion>();

            return manager;
        });

        return services;
    }
}