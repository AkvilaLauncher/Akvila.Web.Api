using Akvila.Web.Api.Core.Repositories;
using Akvila.Web.Api.Dto.Settings;
using AutoMapper;

namespace Akvila.Web.Api.Core.Handlers;

public interface ISettingsHandler {
    static abstract Task<IResult> UpdateSettings(
        ISettingsRepository settingsService,
        IMapper mapper,
        SettingsUpdateDto settingsDto);

    static abstract Task<IResult> GetSettings(
        ISettingsRepository settingsService,
        IMapper mapper);
}