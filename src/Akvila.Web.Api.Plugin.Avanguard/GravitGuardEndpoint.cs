using System.Threading.Tasks;
using Akvila.Web.Api.EndpointSDK;
using Akvila.Web.Api.Plugin.Avanguard.Core;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Akvila.Web.Api.Plugin.Avanguard;

[Path("post", "/api/v1/plugins/avanguard/compile", true)]
public class GravitGuardEndpoint : IPluginEndpoint {
    public async Task Execute(HttpContext context, IAkvilaManager akvilaManager) {
        await context.Response.WriteAsync("templaацуфацфуаte");
    }
}