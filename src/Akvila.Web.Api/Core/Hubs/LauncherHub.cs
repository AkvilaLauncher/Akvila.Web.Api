using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Akvila.Web.Api.Core.Hubs.Controllers;
using Akvila.Web.Api.Domains.User;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Akvila.Web.Api.Core.Hubs;

public class LauncherHub : BaseHub {
    private readonly IAkvilaManager _akvilaManager;
    private readonly HubEvents _hubEvents;
    private readonly PlayersController _playerController;
    private static IDisposable? _profilesChangedEvent;

    public LauncherHub(
        IAkvilaManager akvilaManager,
        PlayersController playerController,
        HubEvents hubEvents) {
        _akvilaManager = akvilaManager;
        _hubEvents = hubEvents;
        _playerController = playerController;

        _profilesChangedEvent ??= akvilaManager.Profiles.ProfilesChanged.Subscribe(eventType => {
            foreach (var connection in _playerController.LauncherInfos.Values.Select(c => c.Connection)
                         .OfType<ISingleClientProxy>()) {
                connection?.SendAsync("RefreshProfiles");
            }
        });
    }

    public void ConfirmLauncherHash(string hash) {
        _playerController.ConfirmLauncherHash(Context.ConnectionId, hash);
    }

    public override Task OnConnectedAsync() {
        if (Context.User is null) {
            return Task.CompletedTask;
        }

        _ = _playerController.AddLauncherConnection(Context.ConnectionId, Clients.Caller, Context.User);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        _playerController.RemoveLauncherConnection(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}