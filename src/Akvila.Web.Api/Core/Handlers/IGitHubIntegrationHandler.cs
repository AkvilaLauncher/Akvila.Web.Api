using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Dto.Launcher;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface IGitHubIntegrationHandler {
    static abstract Task<IResult> GetVersions(IGitHubService gitHubService);

    static abstract Task<IResult> DownloadLauncher(
        IAkvilaManager manager,
        IGitHubService gitHubService,
        IValidator<LauncherCreateDto> launcherValidator,
        LauncherCreateDto launcherCreateDto);

    static abstract Task<IResult> ReturnLauncherSolution(IAkvilaManager akvilaManager, string branchName);
}