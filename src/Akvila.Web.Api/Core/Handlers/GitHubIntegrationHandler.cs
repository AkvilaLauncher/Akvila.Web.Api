using System.IO.Compression;
using System.Net;
using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Domains.LauncherDto;
using Akvila.Web.Api.Dto.Launcher;
using Akvila.Web.Api.Dto.Messages;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public class GitHubIntegrationHandler : IGitHubIntegrationHandler {
    private const string LauncherGitHubUrl = "https://github.com/AkvilaLauncher/Akvila.Backend.git";

    public static async Task<IResult> GetVersions(IGitHubService gitHubService) {
        var versions = await gitHubService.GetRepositoryTags("AkvilaLauncher", "Akvila.Launcher");

        var versionsDtos = versions.Select(c => new LauncherVersionReadDto {
            Version = c
        });

        return Results.Ok(ResponseMessage.Create(versionsDtos, "Version list successfully received", HttpStatusCode.OK));
    }

    public static async Task<IResult> DownloadLauncher(
        IAkvilaManager manager,
        IGitHubService gitHubService,
        IValidator<LauncherCreateDto> validator,
        LauncherCreateDto launcherCreateDto) {
        var result = await validator.ValidateAsync(launcherCreateDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        var path = Path.Combine(manager.LauncherInfo.InstallationDirectory, "Launcher");

        var projectPath =
            await gitHubService.DownloadProject(path, launcherCreateDto.GitHubVersions, LauncherGitHubUrl);

        await gitHubService.EditLauncherFiles(projectPath, launcherCreateDto.Host, launcherCreateDto.Folder);

        return await ReturnLauncherSolution(manager, launcherCreateDto.GitHubVersions);
    }

    public static async Task<IResult> ReturnLauncherSolution(IAkvilaManager akvilaManager, string version) {
        var projectPath = Path.Combine(akvilaManager.LauncherInfo.InstallationDirectory, "Launcher", version);

        if (!Directory.Exists(projectPath))
            return Results.BadRequest(ResponseMessage.Create("Project not found, download and build first",
                HttpStatusCode.BadRequest));

        var zipPath = Path.Combine(Path.GetTempPath(), $"Solution_Launcher_{DateTime.Now.Ticks}.zip");

        await Task.Run(() => ZipFile.CreateFromDirectory(projectPath, zipPath));

        var contentType = "application/zip";

        var downloadFileName = "akvila-solution.zip";

        var fileBytes = await File.ReadAllBytesAsync(zipPath);

        return Results.File(fileBytes, contentType, downloadFileName);
    }
}