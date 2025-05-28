using System.Collections.Concurrent;
using System.Diagnostics;
using Akvila.Web.Api.Core.Hubs.Controllers;
using Akvila.Web.Api.Domains.User;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Akvila.Web.Api.Core.Hubs;

public class GameServerHub(
    IAkvilaManager akvilaManager,
    PlayersController playerController,
    HubEvents hubEvents)
    : BaseHub {
    public override Task OnConnectedAsync() {
        playerController.GameServersConnections.TryAdd(Context.ConnectionId, Clients.Caller);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        playerController.GameServersConnections.TryRemove(Context.ConnectionId, out _);

        return base.OnDisconnectedAsync(exception);
    }


    public async Task OnJoin(string userName) {
        try {
            if (!playerController.GetLauncherConnection(userName, out var launcherInfo) ||
                launcherInfo!.ExpiredDate < DateTimeOffset.Now) {
                hubEvents.KickUser.OnNext((userName,
                    "Failed to identify the user. Restart the game together with the Launcher!"));
                return;
            }

            Debug.WriteLine($"OnJoin: {userName}; ExpiredTime: {launcherInfo.ExpiredDate - DateTimeOffset.Now:g}");
            var user = await akvilaManager.Users.GetUserByName(userName);

            if (user is null) {
                await Clients.Caller.SendAsync("BanUser", userName);
                return;
            }

            await akvilaManager.Users.StartSession(user);
        }
        catch (Exception e) {
            hubEvents.KickUser.OnNext((userName, "An error occurred while trying to connect to the server"));
            Console.WriteLine(e);
        }
    }

    public async Task OnLeft(string userName) {
        try {
            if (!playerController.GetLauncherConnection(userName, out var launcherInfo) ||
                launcherInfo!.ExpiredDate < DateTimeOffset.Now) {
                return;
            }

            var user = await akvilaManager.Users.GetUserByName(userName);

            if (user is null) {
                await Clients.Caller.SendAsync("BanUser", userName);
                return;
            }

            Debug.WriteLine($"OnLeft: {userName}");
            await akvilaManager.Users.EndSession(user);
        }
        catch (Exception e) {
            hubEvents.KickUser.OnNext((userName, "An error occurred while trying to connect to the server"));
            Console.WriteLine(e);
        }
    }
}