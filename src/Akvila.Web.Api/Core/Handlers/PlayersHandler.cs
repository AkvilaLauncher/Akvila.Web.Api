using System.Collections.Frozen;
using System.Net;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Player;
using AutoMapper;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public class PlayersHandler : IPlayersHandler {
    public static async Task<IResult> GetPlayers(IAkvilaManager akvilaManager, IMapper mapper, int? take, int? offset,
        string? findName) {
        var players = await akvilaManager.Users.GetUsers(take ?? 20, offset ?? 0, findName ?? string.Empty);

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ExtendedPlayerReadDto>>(players),
            "User list successfully received", HttpStatusCode.OK));
    }

    public static async Task<IResult> BanPlayer(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IList<string> playerUuids,
        bool deviceBlock = false) {
        if (!playerUuids.Any()) {
            return Results.BadRequest(ResponseMessage.Create("No user has been transferred for blocking",
                HttpStatusCode.BadRequest));
        }

        foreach (var playerUuid in playerUuids) {
            var player = await akvilaManager.Users.GetUserByUuid(playerUuid);

            if (player is null) continue;
            player.IsBanned = true;
            await akvilaManager.Users.UpdateUser(player);
        }

        return Results.Ok(ResponseMessage.Create("User(s) successfully blocked", HttpStatusCode.OK));
    }

    public static async Task<IResult> RemovePlayer(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IList<string> playerUuids) {
        if (!playerUuids.Any()) {
            return Results.BadRequest(ResponseMessage.Create("No user has been transferred for blocking",
                HttpStatusCode.BadRequest));
        }

        var profiles = (await akvilaManager.Profiles.GetProfiles()).ToFrozenSet();


        foreach (var playerUuid in playerUuids) {
            var player = await akvilaManager.Users.GetUserByUuid(playerUuid);

            if (player is null) continue;

            if (profiles.Any(c => c.UserWhiteListGuid.Contains(playerUuid))) {
                return Results.BadRequest(ResponseMessage.Create(
                    $"The user \"{player.Name}\" is whitelisted, remove him from all profiles before deleting him!",
                    HttpStatusCode.BadRequest));
            }

            await akvilaManager.Users.RemoveUser(player);
        }

        return Results.Ok(ResponseMessage.Create("User(s) successfully locked", HttpStatusCode.OK));
    }

    public static async Task<IResult> PardonPlayer(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IList<string> playerUuids) {
        if (!playerUuids.Any()) {
            return Results.BadRequest(ResponseMessage.Create("No user has been transferred for blocking",
                HttpStatusCode.BadRequest));
        }

        foreach (var playerUuid in playerUuids) {
            var player = await akvilaManager.Users.GetUserByUuid(playerUuid);

            if (player is null) continue;
            player.IsBanned = false;
            await akvilaManager.Users.UpdateUser(player);
        }

        return Results.Ok(ResponseMessage.Create("User(s) successfully unblocked", HttpStatusCode.OK));
    }
}