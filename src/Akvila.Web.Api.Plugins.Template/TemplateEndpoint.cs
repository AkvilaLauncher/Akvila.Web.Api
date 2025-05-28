using System.Threading.Tasks;
using Akvila.Web.Api.EndpointSDK;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Akvila.Web.Api.Plugins.Template;

[Path("get", "/api/v1/plugins/template", true)]
public class TemplateEndpoint : IPluginEndpoint {
    public async Task Execute(HttpContext context, IAkvilaManager akvilaManager) {
        await context.Response.WriteAsync("templaацуфацфуаte");
    }
}