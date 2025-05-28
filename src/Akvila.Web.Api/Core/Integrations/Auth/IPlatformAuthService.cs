using Akvila.Web.Api.Domains.Integrations;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public interface IPlatformAuthService {
    Task<AuthResult> Auth(string login, string password);
}