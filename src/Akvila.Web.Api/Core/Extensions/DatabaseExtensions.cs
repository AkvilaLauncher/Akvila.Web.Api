using System.Reactive.Subjects;
using Akvila.Web.Api.Core.Options;
using Akvila.Web.Api.Data;
using Akvila.Web.Api.Domains.Settings;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using Akvila.Core.Launcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Akvila.Web.Api.Core.Extensions;

public static class DatabaseExtensions {
    public static WebApplication InitializeDatabase(this WebApplication app) {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<DatabaseContext>();
        var settings = services.GetRequiredService<ServerSettings>();

        app.UseCors(settings.PolicyName);

        if (context.Database.GetPendingMigrations().Any())
            context.Database.Migrate();

        EnsureCreateRecords(context, app.Services);

        return app;
    }

    private static void EnsureCreateRecords(DatabaseContext context, IServiceProvider services) {
        var settingsSubject = services.GetRequiredService<ISubject<Settings>>();
        var gmlManager = services.GetRequiredService<IAkvilaManager>();
        var applicationContext = services.GetRequiredService<ApplicationContext>();

        var dataBaseSettings = context.Settings.OrderBy(c => c.Id).LastOrDefault();

        if (dataBaseSettings is null) {
            dataBaseSettings = context.Settings.Add(new Settings {
                RegistrationIsEnabled = true,
                CurseForgeKey = string.Empty,
                TextureProtocol = TextureProtocol.Https
            }).Entity;

            context.SaveChanges();
        }

        settingsSubject.OnNext(dataBaseSettings);

        RestoreStorage(gmlManager, dataBaseSettings);

        gmlManager.LauncherInfo.UpdateSettings(
            dataBaseSettings.StorageType,
            dataBaseSettings.StorageHost,
            dataBaseSettings.StorageLogin,
            dataBaseSettings.StoragePassword,
            dataBaseSettings.TextureProtocol,
            dataBaseSettings.CurseForgeKey,
            dataBaseSettings.VkKey
        );
    }

    private static void RestoreStorage(IAkvilaManager akvilaManager, Settings settings) {
        akvilaManager.LauncherInfo.StorageSettings.StorageType = settings.StorageType;
        akvilaManager.LauncherInfo.StorageSettings.StorageHost = settings.StorageHost;
        akvilaManager.LauncherInfo.StorageSettings.StorageLogin = settings.StorageLogin;
        akvilaManager.LauncherInfo.StorageSettings.StoragePassword = settings.StoragePassword;
        akvilaManager.LauncherInfo.StorageSettings.TextureProtocol = settings.TextureProtocol;
        akvilaManager.LauncherInfo.AccessTokens[AccessTokenTokens.CurseForgeKey] = settings.CurseForgeKey;
        akvilaManager.LauncherInfo.AccessTokens[AccessTokenTokens.VkKey] = settings.VkKey;
    }
}