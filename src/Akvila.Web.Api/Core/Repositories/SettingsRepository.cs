using System.Reactive.Subjects;
using Akvila.Web.Api.Core.Options;
using Akvila.Web.Api.Data;
using Akvila.Web.Api.Domains.Settings;
using AkvilaCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Akvila.Web.Api.Core.Repositories;

public class SettingsRepository(
    DatabaseContext databaseContext,
    ServerSettings options,
    IAkvilaManager akvilaManager,
    ISubject<Settings> settingsObservable)
    : ISettingsRepository {
    private readonly ServerSettings _options = options;
    public IObservable<Settings> SettingsUpdated => settingsObservable;

    public async Task<Settings?> UpdateSettings(Settings settings) {
        akvilaManager.LauncherInfo.UpdateSettings(
            settings.StorageType,
            settings.StorageHost,
            settings.StorageLogin,
            settings.StoragePassword,
            settings.TextureProtocol,
            settings.CurseForgeKey,
            settings.VkKey
        );

        await databaseContext.AddAsync(settings);
        await databaseContext.SaveChangesAsync();

        settingsObservable.OnNext(settings);

        return settings;
    }

    public Task<Settings?> GetSettings() {
        return databaseContext.Settings.OrderByDescending(c => c.Id).FirstOrDefaultAsync();
    }
}