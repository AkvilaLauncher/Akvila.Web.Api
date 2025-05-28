using Akvila.Web.Api.Core.Extensions;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class AuthService(IAuthServiceFactory authServiceFactory) : IAuthService {
    public Task<AuthResult> CheckAuth(string login, string password, AuthType authType) {
        var authService = authServiceFactory.CreateAuthService(authType);

        return authService.Auth(login, password);
    }
}