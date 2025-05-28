using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class NamelessMCAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : IPlatformAuthService {
    public Task<AuthResult> Auth(string login, string password) {
        throw new NotImplementedException();
    }
}