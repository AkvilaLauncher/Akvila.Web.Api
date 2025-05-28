using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface ILauncherUpdateHandler {
    static abstract Task<IResult> UploadLauncherVersion(HttpContext context, IAkvilaManager akvilaManager);
}