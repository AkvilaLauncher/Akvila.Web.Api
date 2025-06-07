using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class WordPressAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : CustomEndpointAuthService(httpClientFactory, akvilaManager);
