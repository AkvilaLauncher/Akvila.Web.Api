using System.ComponentModel;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Akvila.Web.Api.Core.Hubs;

public class ProfileHub : BaseHub {
    private readonly IAkvilaManager _akvilaManager;
    private double lastPackProgress = -1;

    private double lastProgress = -1;

    public ProfileHub(IAkvilaManager akvilaManager) {
        _akvilaManager = akvilaManager;
    }

    public async Task Build(string profileName) {
        try {
            if (string.IsNullOrEmpty(profileName))
                return;

            var profile = await _akvilaManager.Profiles.GetProfile(profileName);

            if (profile is null)
                return;

            if (profile.State is ProfileState.Loading or ProfileState.Packing) {
                SendCallerMessage(
                    "At this point, the selected profile is already being downloaded!");
                return;
            }

            Log("Preparation for packaging...", profileName);

            var eventInfo = _akvilaManager.Profiles.PackChanged.Subscribe(percentage => {
                ChangePackProgress(profileName, percentage);
            });
            await _akvilaManager.Profiles.PackProfile(profile);
            await Clients.All.SendAsync("SuccessPacked", profileName);
            eventInfo.Dispose();
            lastPackProgress = -1;
        }
        catch (Exception exception) {
            Console.WriteLine(exception);
            throw;
        }
    }

    private async void ChangePackProgress(string profileName, double percentage) {
        try {
            if (Math.Abs(lastPackProgress - percentage) < 0.001) return;

            lastPackProgress = percentage;

            await Clients.All.SendAsync("ChangeProgress", profileName, percentage);
        }
        catch (Exception exception) {
            SendCallerMessage(exception.Message);
            Console.WriteLine(exception);
        }
    }

    public async Task Restore(string profileName) {
        try {
            var profile = await _akvilaManager.Profiles.GetProfile(profileName);

            if (profile == null) {
                SendCallerMessage($"\"{profileName}\" profile was not found");
                return;
            }

            if (profile.State == ProfileState.Loading) {
                SendCallerMessage(
                    "At this point, the selected profile is already being uploaded!");
                return;
            }

            var fullPercentage = profile.GameLoader.FullPercentages.Subscribe(percentage => {
                SendProgress("FullProgress", profile.Name, percentage);
            });

            var loadPercentage = profile.GameLoader.LoadPercentages.Subscribe(percentage => {
                SendProgress("ChangeProgress", profile.Name, percentage);
            });

            var logInfo = profile.GameLoader.LoadLog.Subscribe(logs => { Log(logs, profile.Name); });

            var exception = profile.GameLoader.LoadException.Subscribe(async logs => {
                try {
                    await Clients.All.SendAsync("OnException", profile.Name, logs.ToString());
                }
                catch (Exception exception) {
                    Console.WriteLine(exception);
                    _akvilaManager.BugTracker.CaptureException(exception);
                }
            });

            await _akvilaManager.Profiles.RestoreProfileInfo(profile.Name);

            await Clients.All.SendAsync("SuccessInstalled", profile.Name);

            fullPercentage.Dispose();
            loadPercentage.Dispose();
            logInfo.Dispose();
            exception.Dispose();

            lastProgress = -1;
        } catch (Exception exception) {
            _akvilaManager.BugTracker.CaptureException(exception);
            SendCallerMessage($"Failed to restore profile. {exception.Message}");
            Console.WriteLine(exception);
        }
    }

    private async void SendProgress(string name, string profileName, double percentage) {
        try {
            if (Math.Abs(lastProgress - percentage) < 0.000) return;

            var percentageValue = Math.Round(percentage, 2);

            if (double.IsPositiveInfinity(percentageValue) || double.IsNegativeInfinity(percentageValue)) {
                return;
            }

            if (double.IsNaN(percentageValue) || double.IsNaN(percentageValue)) {
                return;
            }

            lastProgress = percentage;
            await Clients.All.SendAsync(name, profileName, percentageValue);
        } catch (Exception exception) {
            Console.WriteLine(exception);
        }
    }

    public class ConfigureJsonOptions : IConfigureOptions<JsonOptions> {
        public void Configure(JsonOptions options) {
            options.SerializerOptions.NumberHandling =
                System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
        }
    }
}