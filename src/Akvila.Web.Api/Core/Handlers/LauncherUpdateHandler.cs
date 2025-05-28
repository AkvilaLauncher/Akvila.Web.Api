using System.Net;
using Akvila.Web.Api.Domains.Launcher;
using Akvila.Web.Api.Dto.Launcher;
using Akvila.Web.Api.Dto.Messages;
using AutoMapper;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Launcher;

namespace Akvila.Web.Api.Core.Handlers;

public class LauncherUpdateHandler : ILauncherUpdateHandler {
    public static IResult GetActualVersion(IAkvilaManager akvilaManager) {
        return Results.Ok(ResponseMessage.Create(akvilaManager.LauncherInfo.ActualLauncherVersion, string.Empty,
            HttpStatusCode.OK));
    }

    public static async Task<IResult> GetBuilds(IAkvilaManager akvilaManager, IMapper mapper) {
        var builds = await akvilaManager.LauncherInfo.GetBuilds();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<LauncherBuildReadDto>>(builds), string.Empty,
            HttpStatusCode.OK));
    }

    public static async Task<IResult> GetPlatforms(IAkvilaManager akvilaManager) {
        var platforms = await akvilaManager.Launcher.GetPlatforms();

        return Results.Ok(ResponseMessage.Create(platforms, string.Empty, HttpStatusCode.OK));
    }

    public static async Task<IResult> UploadLauncherVersion(HttpContext context, IAkvilaManager akvilaManager) {
        var buildName = context.Request.Form["LauncherBuild"].ToString();

        if (string.IsNullOrEmpty(buildName)) {
            return Results.BadRequest(ResponseMessage.Create($"Failed to determine the build version",
                HttpStatusCode.BadRequest));
        }

        var build = await akvilaManager.LauncherInfo.GetBuild(buildName);

        if (build is null) {
            return Results.BadRequest(ResponseMessage.Create($"The specified version was not found",
                HttpStatusCode.BadRequest));
        }

        var minecraftVersion = new LauncherVersion {
            Version = context.Request.Form["Version"].FirstOrDefault() ?? string.Empty,
            Title = context.Request.Form["Title"].FirstOrDefault() ?? string.Empty,
            Description = context.Request.Form["Description"].FirstOrDefault() ?? string.Empty
        };

        try {
            var versions = await akvilaManager.Launcher.CreateVersion(minecraftVersion, build);

            return Results.Ok(ResponseMessage.Create("Launcher version successfully updated!", HttpStatusCode.OK));
        } catch (Exception exception) {
            await akvilaManager.Notifications.SendMessage("Error when publishing a new version of the launcher", exception);
            return Results.BadRequest(ResponseMessage.Create($"Failed to update the launcher file: {exception.Message}",
                HttpStatusCode.InternalServerError));
        }
    }
}