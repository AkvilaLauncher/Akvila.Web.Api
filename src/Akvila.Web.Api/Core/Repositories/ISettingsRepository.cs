using Akvila.Web.Api.Domains.Settings;

namespace Akvila.Web.Api.Core.Repositories;

public interface ISettingsRepository {
    Task<Settings?> UpdateSettings(Settings settings);
    Task<Settings?> GetSettings();
    IObservable<Settings> SettingsUpdated { get; }
}