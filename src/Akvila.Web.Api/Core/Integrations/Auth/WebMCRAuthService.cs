using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class WebMCRAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : CustomEndpointAuthService(httpClientFactory, akvilaManager);
