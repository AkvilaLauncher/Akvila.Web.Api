using System.Net;
using Akvila.Web.Api.Core.Options;

namespace Akvila.Web.Api.Core.Extensions;

public static class HttpClientsExtensions
{
    public static IServiceCollection AddNamedHttpClients(this IServiceCollection services)
    {
        string? skinsServiceUrl = Environment.GetEnvironmentVariable("SkinServiceUrl");

        var dockerEnv = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");

        var isRunningInDocker = !string.IsNullOrEmpty(dockerEnv) && dockerEnv.ToLower() == "true";

        services.AddHttpClient(HttpClientNames.SkinService, client =>
        {
            client.BaseAddress = isRunningInDocker
                ? new Uri("http://akvila-web-skins:8085/")
                : string.IsNullOrEmpty(skinsServiceUrl) ? null : new Uri(skinsServiceUrl);
        });

        return services;
    }
    public static IPAddress? ParseRemoteAddress(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) ||
            string.IsNullOrWhiteSpace(forwardedFor))
            return context.Connection.RemoteIpAddress;

        var address = forwardedFor.ToString().Split(',').FirstOrDefault() ?? string.Empty;

        return IPAddress.TryParse(address, out var ipAddress)
            ? ipAddress
            : context.Connection.RemoteIpAddress;
    }

}
