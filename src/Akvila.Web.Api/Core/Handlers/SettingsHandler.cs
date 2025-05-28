using System.Net;
using Akvila.Web.Api.Core.Repositories;
using Akvila.Web.Api.Domains.Settings;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Settings;
using AutoMapper;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public abstract class SettingsHandler : ISettingsHandler {
    public static async Task<IResult> UpdateSettings(
        ISettingsRepository settingsService,
        IMapper mapper,
        SettingsUpdateDto settingsDto) {
        var settings = mapper.Map<Settings>(settingsDto);

        var result = await settingsService.UpdateSettings(settings);

        return Results.Ok(ResponseMessage.Create(
            mapper.Map<SettingsReadDto>(result),
            string.Empty,
            HttpStatusCode.OK));
    }

    public static async Task<IResult> GetSettings(ISettingsRepository settingsService, IMapper mapper) {
        var settings = await settingsService.GetSettings();

        return Results.Ok(ResponseMessage.Create(
            mapper.Map<SettingsReadDto>(settings),
            "Settings received",
            HttpStatusCode.OK));
    }
}