using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public interface IAuthServiceFactory {
    IPlatformAuthService CreateAuthService(AuthType platformKey);
}