using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class NamelessMCAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : CustomEndpointAuthService(httpClientFactory, akvilaManager);
