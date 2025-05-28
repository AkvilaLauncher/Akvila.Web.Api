using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public interface IAuthService {
    Task<AuthResult> CheckAuth(string login, string password, AuthType authType);
}