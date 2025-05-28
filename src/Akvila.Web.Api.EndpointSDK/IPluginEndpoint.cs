using System.Threading.Tasks;
using AkvilaCore.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Akvila.Web.Api.EndpointSDK;

public interface IPluginEndpoint {
    Task Execute(HttpContext context, IAkvilaManager akvilaManager);
}