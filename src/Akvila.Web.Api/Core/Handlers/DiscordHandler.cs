using System.Net;
using Akvila.Web.Api.Domains.Integrations;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.Messages;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Akvila.Web.Api.Core.Handlers;

public class DiscordHandler : IDiscordHandler {
    public static async Task<IResult> GetInfo(IAkvilaManager akvilaManager, IMapper mapper) {
        var discordRpcInfo = await akvilaManager.Integrations.GetDiscordRpc();

        if (discordRpcInfo is null)
            return Results.NotFound(ResponseMessage.Create("DiscordRPC service is not configured", HttpStatusCode.NotFound));

        return Results.Ok(ResponseMessage.Create(mapper.Map<DiscordRpcReadDto>(discordRpcInfo),
            "DiscordRPC service successfully received", HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> UpdateInfo(IAkvilaManager akvilaManager, IMapper mapper,
        IValidator<DiscordRpcUpdateDto> validator, DiscordRpcUpdateDto discordRpcUpdateDto) {
        var result = await validator.ValidateAsync(discordRpcUpdateDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        await akvilaManager.Integrations.UpdateDiscordRpc(mapper.Map<DiscordRpcClient>(discordRpcUpdateDto));

        return Results.Ok(ResponseMessage.Create("DiscordRPC service successfully updated", HttpStatusCode.OK));
    }
}