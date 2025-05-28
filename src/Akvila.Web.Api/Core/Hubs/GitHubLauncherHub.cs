using System.Diagnostics;
using System.Runtime.InteropServices;
using Akvila.Web.Api.Core.Services;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Akvila.Web.Api.Core.Hubs;

public class GitHubLauncherHub(IGitHubService gitHubService, IAkvilaManager akvilaManager) : BaseHub {
    private const string _launcherGitHub = "https://github.com/AkvilaLauncher/Akvila.Launcher";

    public async Task Download(string branchName, string host, string folderName) {
        try {
            var projectPath = Path.Combine(akvilaManager.LauncherInfo.InstallationDirectory, "Launcher", branchName);

            if (Directory.Exists(projectPath)) {
                await akvilaManager.Notifications
                    .SendMessage("Launcher already exists in the folder, delete it before building it",
                        NotificationType.Error);
                return;
            }

            projectPath = Path.Combine(akvilaManager.LauncherInfo.InstallationDirectory, "Launcher");

            ChangeProgress(nameof(GitHubLauncherHub), 5);
            var allowedVersions = await gitHubService
                .GetRepositoryTags("AkvilaLauncher", "Akvila.Launcher");

            if (allowedVersions.All(c => c != branchName)) {
                await akvilaManager.Notifications
                    .SendMessage($"The received version of the launcher \"{branchName}\" is unsupported",
                        NotificationType.Error);
                return;
            }

            ChangeProgress(nameof(GitHubLauncherHub), 10);
            var newFolder = await gitHubService.DownloadProject(projectPath, branchName, _launcherGitHub);
            ChangeProgress(nameof(GitHubLauncherHub), 20);

            await gitHubService.EditLauncherFiles(newFolder, host, folderName);
            ChangeProgress(nameof(GitHubLauncherHub), 30);

            ChangeProgress(nameof(GitHubLauncherHub), 100);
            SendCallerMessage($"The \"{branchName}\" project has been successfully created.");
        } catch (Exception exception) {
            Console.WriteLine(exception);
            await akvilaManager.Notifications.SendMessage("Error when loading the Launcher client", exception);
        } finally {
            await Clients.Caller.SendAsync("LauncherDownloadEnded");
        }
    }

    public async Task Compile(string version, string[] osTypes) {
        try {
            if (!akvilaManager.Launcher.CanCompile(version, out string message)) {
                SendCallerMessage(message);
                return;
            }

            Log("Start compilling...");

            if (await akvilaManager.LauncherInfo.Settings.SystemProcedures.InstallDotnet()) {
                var eventObservable = akvilaManager.Launcher.BuildLogs.Subscribe(Log);

                var result = await akvilaManager.Launcher.Build(version, osTypes);

                eventObservable.Dispose();

                if (result) {
                    await akvilaManager.Notifications.SendMessage("Launcher successfully compiled!",
                        NotificationType.Info);
                } else {
                    await akvilaManager.Notifications.SendMessage("Launcher build ended with an error!",
                        NotificationType.Error);
                }
            }
        } catch (Exception exception) {
            Console.WriteLine(exception);
            await akvilaManager.Notifications.SendMessage("Error when loading the Launcher client", exception);
        } finally {
            await Clients.Caller.SendAsync("LauncherBuildEnded");
        }
    }
}