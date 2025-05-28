using System.Net;
using System.Text;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Servers;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;
using Akvila.Models.Servers;

namespace Akvila.Web.Api.Core.Handlers;

internal abstract class ServersHandler {
    public static async Task<IResult> GetServers(IAkvilaManager akvilaManager, IMapper mapper, string profileName) {
        if (string.IsNullOrEmpty(profileName))
            return Results.BadRequest(ResponseMessage.Create("Passed an empty parameter as a profile name",
                HttpStatusCode.BadRequest));

        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.BadRequest(ResponseMessage.Create("The profile with this name does not exist",
                HttpStatusCode.BadRequest));

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ServerReadDto>>(profile.Servers), string.Empty,
            HttpStatusCode.OK));
    }

    public static async Task<IResult> RemoveServer(IAkvilaManager akvilaManager, string profileName,
        string serverNamesString) {
        if (string.IsNullOrEmpty(profileName))
            return Results.BadRequest(ResponseMessage.Create("Passed an empty parameter as a profile name",
                HttpStatusCode.BadRequest));

        if (string.IsNullOrEmpty(serverNamesString))
            return Results.BadRequest(ResponseMessage.Create("Passed an empty parameter as the server name",
                HttpStatusCode.BadRequest));

        var serverNames = serverNamesString.Split(',');

        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.BadRequest(ResponseMessage.Create("The profile with this name does not exist",
                HttpStatusCode.BadRequest));

        int amount = 0;
        foreach (var serverName in serverNames) {
            var server = profile.Servers.FirstOrDefault(c => c.Name == serverName);

            if (server is null) {
                continue;
            }

            profile.RemoveServer(server);

            await akvilaManager.Profiles.SaveProfiles();
            amount++;
        }

        return Results.Ok(ResponseMessage.Create($"Processing completed, total deleted: {amount}", HttpStatusCode.OK));
    }

    public static async Task<IResult> CreateServer(
        IAkvilaManager akvilaManager,
        IValidator<CreateServerDto> validator,
        IMapper mapper,
        string profileName,
        CreateServerDto createDto) {
        try {
            if (string.IsNullOrEmpty(profileName))
                return Results.BadRequest(ResponseMessage.Create(
                    "Passed an empty parameter as a profile name",
                    HttpStatusCode.BadRequest));

            var result = await validator.ValidateAsync(createDto);

            if (!result.IsValid)
                return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                    HttpStatusCode.BadRequest));

            var profile = await akvilaManager.Profiles.GetProfile(profileName);

            if (profile is null)
                return Results.BadRequest(ResponseMessage.Create("The profile with this name does not exist",
                    HttpStatusCode.BadRequest));

            var mappedServer = mapper.Map<MinecraftServer>(createDto);
            mappedServer.ServerProcedures = akvilaManager.Servers;

            profile.AddServer(mappedServer);

            await akvilaManager.Profiles.SaveProfiles();

            var resultObject = ResponseMessage.Create(mapper.Map<ServerReadDto>(mappedServer),
                "Server successfully added",
                HttpStatusCode.Created);

            return Results.Created($"/api/v1/servers/{profileName}/{mappedServer.Name}", resultObject);
        }
        catch (Exception exception) {
            return Results.BadRequest(ResponseMessage.Create(exception.Message, HttpStatusCode.BadRequest));
        }
    }
}