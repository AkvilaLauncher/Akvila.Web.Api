using System.Net;
using Akvila.Web.Api.Core.Handlers;
using Akvila.Web.Api.Core.Hubs;
using Akvila.Web.Api.Domains.LauncherDto;
using Akvila.Web.Api.Domains.Plugins;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.News;
using Akvila.Web.Api.Dto.Player;
using Akvila.Web.Api.Dto.Profile;
using Akvila.Web.Api.Dto.Servers;
using Akvila.Web.Api.Dto.Settings;
using Akvila.Web.Api.Dto.User;
using AkvilaCore.Interfaces.Notifications;
using AkvilaCore.Interfaces.User;

namespace Akvila.Web.Api.Core.Extensions;

public static class EndpointsExtensions {
    public static WebApplication RegisterEndpoints(this WebApplication app) {
        #region Root

        app.MapGet("/", () => Results.Redirect("/swagger", true));

        #endregion

        #region Launcher

        app.MapGet("/api/v1/integrations/github/launcher/versions", GitHubIntegrationHandler.GetVersions)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "List of Launcher versions";
                return generatedOperation;
            })
            .WithName("Get launcher versions")
            .WithTags("Integration/GitHub/Launcher")
            .Produces<ResponseMessage<IEnumerable<LauncherVersionReadDto>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/integrations/github/launcher/download", GitHubIntegrationHandler.DownloadLauncher)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Download Launcher";
                return generatedOperation;
            })
            .WithName("Download launcher version")
            .WithTags("Integration/GitHub/Launcher")
            .Produces<ResponseMessage<string>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/github/launcher/download/{version}",
                GitHubIntegrationHandler.ReturnLauncherSolution)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Download Launcher Solution";
                return generatedOperation;
            })
            .WithName("Download launcher solution")
            .WithTags("Integration/GitHub/Launcher")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region SignalR Hubs

        app.MapHub<ProfileHub>("/ws/profiles/restore")
            .RequireAuthorization(c => c.RequireRole("Admin"));
        app.MapHub<GitHubLauncherHub>("/ws/launcher/build")
            .RequireAuthorization(c => c.RequireRole("Admin"));
        app.MapHub<GameServerHub>("/ws/gameServer")
            .RequireAuthorization(c => c.RequireRole("Admin"));
        app.MapHub<LauncherHub>("/ws/launcher").RequireAuthorization();
        app.MapHub<NotificationHub>("/ws/notifications")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Auth

        app.MapPost("/api/v1/users/signup", AuthHandler.CreateUser)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "New user registration";
                return generatedOperation;
            })
            .WithName("Create User")
            .WithTags("Users")
            .Produces<ResponseMessage<UserAuthReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPost("/api/v1/users/signin", AuthHandler.AuthUser)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "User Authorization";
                return generatedOperation;
            })
            .WithDescription("Authorization")
            .WithName("Authenticate User")
            .WithTags("Users")
            .Produces<ResponseMessage<UserAuthReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/users/info/{userName}", AuthHandler.UserInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting general information about the user";
                return generatedOperation;
            })
            .WithDescription("Getting general information about the user")
            .WithName("User info")
            .WithTags("Users")
            .Produces<ResponseMessage<UserAuthReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        #endregion

        #region Integrations

        #region Sentry

        app.MapGet("/api/v1/integrations/sentry/dsn", SentryErrorSaveHandler.GetDsnUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a DSN for the Launcher";
                return generatedOperation;
            })
            .WithDescription("Getting a link to Sentry's DSN service")
            .WithName("Get dsn sentry service url")
            .WithTags("Integration/Sentry")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPut("/api/v1/integrations/sentry/dsn", SentryErrorSaveHandler.UpdateDsnUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "DSN update for the Launcher";
                return generatedOperation;
            })
            .WithDescription("Updating the link to Sentry's DSN service")
            .WithName("Update dsn sentry service url")
            .WithTags("Integration/Sentry")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/{projectId}/envelope", SentryHandler.CreateBugInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Adding Sentry errors";
                return generatedOperation;
            })
            .WithDescription("Adding Sentry errors")
            .WithName("Get sentry message")
            .WithTags("Integration/Sentry");

        app.MapPost("/api/v1/sentry", SentryHandler.GetBugs)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Receiving all Sentry errors";
                return generatedOperation;
            })
            .WithDescription("Receiving all Sentry errors")
            .WithName("Get all bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/sentry/clear", SentryHandler.SolveAllBugs)
            .WithOpenApi(generatedOperation =>
            {
                generatedOperation.Summary = "Очистка всех ошибок Sentry";
                return generatedOperation;
            })
            .WithDescription("Очистка всех ошибок Sentry")
            .WithName("Clear all bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/sentry/filter", SentryHandler.GetFilterSentry)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a filtered list of errors";
                return generatedOperation;
            })
            .WithDescription("Getting a filtered list of errors")
            .WithName("Get filtered bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/sentry/filter/list", SentryHandler.GetFilterListSentry)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the filtered list by errors";
                return generatedOperation;
            })
            .WithDescription("Getting the filtered list by errors")
            .WithName("Get filtered on bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/sentry/stats/last", SentryHandler.GetLastSentryErrors)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a list of errors for the last 3 months";
                return generatedOperation;
            })
            .WithDescription("Getting a list of errors for the last 3 months")
            .WithName("Get last bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/sentry/stats/summary", SentryHandler.GetSummarySentryErrors)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Get a summary of errors";
                return generatedOperation;
            })
            .WithDescription("Get a summary of errors")
            .WithName("Get summary bugs sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/sentry/{exception}", SentryHandler.GetByException)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting an exception in Sentry";
                return generatedOperation;
            })
            .WithDescription("Getting an exception in Sentry")
            .WithName("Get exception on sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/sentry/bug/{id}", SentryHandler.GetBugId)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a bug on Guid Sentry";
                return generatedOperation;
            })
            .WithDescription("Getting a bug on Guid Sentry")
            .WithName("Get bug or id sentry")
            .WithTags("Integration/Sentry")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Discord

        app.MapGet("/api/v1/integrations/discord", DiscordHandler.GetInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting DiscordRPC";
                return generatedOperation;
            })
            .WithDescription("Retrieving DiscordRPC data")
            .WithName("Get discord RPC data")
            .WithTags("Integration/Discord")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPut("/api/v1/integrations/discord", DiscordHandler.UpdateInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "DiscordRPC update";
                return generatedOperation;
            })
            .WithDescription("DiscordRPC data update")
            .WithName("Update discord RPC data")
            .WithTags("Integration/Discord")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Textures

        app.MapGet("/api/v1/integrations/texture/skins", TextureIntegrationHandler.GetSkinUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a link to a service with skins";
                return generatedOperation;
            })
            .WithDescription("Getting a link to a service with skins")
            .WithName("Get skin texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPut("/api/v1/integrations/texture/skins", TextureIntegrationHandler.SetSkinUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Update the link to the service with skins";
                return generatedOperation;
            })
            .WithDescription("Update the link to the service with skins")
            .WithName("Update skin texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/texture/skins/{textureGuid}", TextureIntegrationHandler.GetUserSkin)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a user skin";
                return generatedOperation;
            })
            .WithDescription("Getting a user skin")
            .WithName("Get user skin texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/texture/capes/{textureGuid}", TextureIntegrationHandler.GetUserCloak)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a user cape";
                return generatedOperation;
            })
            .WithDescription("Getting a user cape")
            .WithName("Get user cloak texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/texture/head/{userUuid}", TextureIntegrationHandler.GetUserHead)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the user's face texture";
                return generatedOperation;
            })
            .WithDescription("Getting the user's face texture")
            .WithName("Get user head texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/texture/cloaks", TextureIntegrationHandler.GetCloakUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a link to the service with cloaks";
                return generatedOperation;
            })
            .WithDescription("Getting a link to the service with cloaks")
            .WithName("Get cloak texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPut("/api/v1/integrations/texture/cloaks", TextureIntegrationHandler.SetCloakUrl)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Update the link to the service with cloaks";
                return generatedOperation;
            })
            .WithDescription("Update the link to the service with cloaks")
            .WithName("Update cloak texture url")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/integrations/texture/skins/load", TextureIntegrationHandler.UpdateUserSkin)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Updating the user's skin";
                return generatedOperation;
            })
            .WithDescription("Updating the user's skin")
            .WithName("Upload skin texture")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPost("/api/v1/integrations/texture/cloaks/load", TextureIntegrationHandler.UpdateUserCloak)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Updating the user's cloak";
                return generatedOperation;
            })
            .WithDescription("Updating the user's cloak")
            .WithName("Upload cloak texture")
            .WithTags("Integration/Textures")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        #endregion

        #region Minecraft authlib

        app.MapGet("/api/v1/integrations/authlib/minecraft", MinecraftHandler.GetMetaData)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Obtaining metadata for Authlib injector";
                return generatedOperation;
            })
            .WithDescription("Obtaining metadata for Authlib injector")
            .WithName("Integration with authlib, get metadata")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);


        app.MapPost("/api/v1/integrations/authlib/minecraft/sessionserver/session/minecraft/join",
                MinecraftHandler.Join)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Implementation of the Minecraft Join method";
                return generatedOperation;
            })
            .WithDescription("Implementation of the Minecraft Join method")
            .WithName("Integration with authlib, join")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/authlib/minecraft/sessionserver/session/minecraft/hasJoined",
                MinecraftHandler.HasJoined)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Implementation of the Minecraft HasJoin method";
                return generatedOperation;
            })
            .WithDescription("Implementation of the Minecraft HasJoin method")
            .WithName("Implementation of Minecraft's HasJoin method")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/authlib/minecraft/sessionserver/session/minecraft/profile/{uuid}",
                MinecraftHandler.GetProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Implementing Minecraft user profile retrieval";
                return generatedOperation;
            })
            .WithDescription("Implementing Minecraft user profile retrieval")
            .WithName("Implementation of Minecraft user profile retrieval")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPost("/api/v1/integrations/authlib/minecraft/profiles/minecraft", MinecraftHandler.GetPlayersUuids)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Implementation of Uuid profiles";
                return generatedOperation;
            })
            .WithDescription("Obtaining metadata for Authlib injector")
            .WithName("Implementation of Uuid profiles")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapGet("/api/v1/integrations/authlib/minecraft/player/attributes", MinecraftHandler.GetPlayerAttribute)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting user attributes";
                return generatedOperation;
            })
            .WithDescription("Obtaining metadata for Authlib injector")
            .WithName(" Getting user attributes")
            .WithTags("Integration/Minecraft/AuthLib")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        #endregion

        #region Auth

        app.MapPost("/api/v1/integrations/auth/signin", AuthIntegrationHandler.Auth)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Authentication via intermediate authorization service";
                return generatedOperation;
            })
            .WithDescription("Authentication via intermediate authorization service")
            .WithName("Auth")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage<PlayerReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPost("/api/v1/integrations/auth/checkToken", AuthIntegrationHandler.AuthWithToken)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Checking whether the authorization token is up to date";
                return generatedOperation;
            })
            .WithDescription("Checking whether the authorization token is up to date")
            .WithName("Auth with access token")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage<PlayerReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPut("/api/v1/integrations/auth", AuthIntegrationHandler.SetAuthService)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Updating information about the intermediate authorization service";
                return generatedOperation;
            })
            .WithDescription("Authorization service update")
            .WithName("Update auth service")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/auth", AuthIntegrationHandler.GetIntegrationServices)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of authorization services";
                return generatedOperation;
            })
            .WithDescription("Getting the list of authorization services")
            .WithName("Auth services list")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage<List<AuthServiceReadDto>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/auth/active", AuthIntegrationHandler.GetAuthService)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Obtaining an active authorization service";
                return generatedOperation;
            })
            .WithDescription("Obtaining an active authorization service")
            .WithName("Get active auth service")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage<AuthServiceReadDto>>()
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/integrations/auth/active", AuthIntegrationHandler.RemoveAuthService)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting an active authorization service";
                return generatedOperation;
            })
            .WithDescription("Deleting an active authorization service")
            .WithName("Remove active auth service")
            .WithTags("Integration/Auth")
            .Produces<ResponseMessage<AuthServiceReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region News

        app.MapPost("/api/v1/integrations/news", NewsHandler.AddNewsListener)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Adding a news listener";
                return generatedOperation;
            })
            .WithDescription("Adding a news listener")
            .WithName("Add news listeners")
            .WithTags("Integration/News")
            .Produces<ResponseMessage>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/integrations/news/{type}", NewsHandler.RemoveNewsListener)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting a news listener";
                return generatedOperation;
            })
            .WithDescription("Deleting a news listener")
            .WithName("Remove news listeners")
            .WithTags("Integration/News")
            .Produces<ResponseMessage>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/news/providers", NewsHandler.GetListeners)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a list of news listeners";
                return generatedOperation;
            })
            .WithDescription("Getting a list of news listeners")
            .WithName("Get news listeners")
            .WithTags("Integration/News")
            .Produces<ResponseMessage>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/news", NewsHandler.GetNewsListener)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of news listener";
                return generatedOperation;
            })
            .WithDescription("Getting the list of news listener")
            .WithName("Get list of news listeners")
            .WithTags("Integration/News")
            .Produces<ResponseMessage>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/integrations/news/list", NewsHandler.GetNews)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of news";
                return generatedOperation;
            })
            .WithDescription("Receiving news")
            .WithName("Get list news")
            .WithTags("Integration/News")
            .Produces<ResponseMessage<NewsGetListenerDto[]>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        #endregion

        #endregion

        #region Profiles

        app.MapGet("/api/v1/profiles", ProfileHandler.GetProfiles)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a list of profiles";
                return generatedOperation;
            })
            .WithDescription("Getting a list of profiles")
            .WithName("Profiles list")
            .WithTags("Profiles")
            .Produces<ResponseMessage<List<ProfileReadDto>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Player", "Admin"));

        app.MapGet("/api/v1/profiles/versions/{gameLoader}/{minecraftVersion}", ProfileHandler.GetMinecraftVersions)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a list of versions for each Minecraft loader";
                return generatedOperation;
            })
            .WithDescription("Getting a list of Minecraft versions")
            .WithName("Minecraft versions")
            .WithTags("Profiles")
            .Produces<ResponseMessage<List<string>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles", ProfileHandler.CreateProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Creating a game profile";
                return generatedOperation;
            })
            .WithDescription("Creating a game profile")
            .WithName("Create profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage<ProfileReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPut("/api/v1/profiles", ProfileHandler.UpdateProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Game Profile Update";
                return generatedOperation;
            })
            .WithDescription("Game Profile Update")
            .WithName("Update profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage<ProfileReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles/restore", ProfileHandler.RestoreProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Installing (loading the server part) of the game profile";
                return generatedOperation;
            })
            .WithDescription("Setting up a game profile")
            .WithName("Restore profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/profiles/{profileNames}", ProfileHandler.RemoveProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting a game profile";
                return generatedOperation;
            })
            .WithDescription("Deleting a game profile")
            .WithName("Remove profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles/{profileName}/players/whitelist/{userUuid}", ProfileHandler.AddPlayerToWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Adding a player to the profile whitelist";
                return generatedOperation;
            })
            .WithDescription("Adding a player to the profile whitelist")
            .WithName("Add users white list profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/profiles/{profileName}/mods", ProfileHandler.GetMods)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of mods in profile";
                return generatedOperation;
            })
            .WithDescription("Getting the list of mods in profile")
            .WithName("Get profile mods")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPut("/api/v1/mods/details", ProfileHandler.UpdateModInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Update on mod information";
                return generatedOperation;
            })
            .WithDescription("Update on mod information")
            .WithName("Update mod details")
            .WithTags("Mods")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/mods/details", ProfileHandler.GetModsDetails)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting information about mods";
                return generatedOperation;
            })
            .WithDescription("Getting information about mods")
            .WithName("Get mod details")
            .WithTags("Mods")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin", "Player"));

        app.MapPost("/api/v1/profiles/{profileName}/mods/load", ProfileHandler.LoadMod)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Uploading mod to profile";
                return generatedOperation;
            })
            .WithDescription("Uploading mod to profile")
            .WithName("Load profile mods")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles/{profileName}/mods/load/url", ProfileHandler.LoadByLink)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Uploading mod to profile by link";
                return generatedOperation;
            })
            .WithDescription("Uploading mod to profile by link")
            .WithName("Load profile mods by link")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/profiles/{profileName}/mods/remove/{fileName}", ProfileHandler.RemoveMod)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting a mod from a profile";
                return generatedOperation;
            })
            .WithDescription("Deleting a mod from a profile")
            .WithName("Remove profile mods")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/profiles/{profileName}/mods/optionals", ProfileHandler.GetOptionalsMods)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of optional mods in profile";
                return generatedOperation;
            })
            .WithDescription("Getting the list of optional mods in profile")
            .WithName("Get optional profile mods")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin", "Player"));

        app.MapGet("/api/v1/profiles/{profileName}/mods/search", ProfileHandler.FindMods)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of mods available for download in the profile";
                return generatedOperation;
            })
            .WithDescription("Getting the list of mods available for download in the profile")
            .WithName("Get available profile mods")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/profiles/{profileName}/mods/info", ProfileHandler.GetModInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Get detailed information on the module";
                return generatedOperation;
            })
            .WithDescription("Get detailed information on the module")
            .WithName("Get mod info")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/profiles/{profileName}/players/whitelist/{userUuid}",
                ProfileHandler.RemovePlayerFromWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Removing a player from the profile whitelist";
                return generatedOperation;
            })
            .WithDescription("Removing a player from the profile whitelist")
            .WithName("Remove user from profile white list")
            .WithTags("Profiles")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles/info", ProfileHandler.GetProfileInfo)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting game profile information";
                return generatedOperation;
            })
            .WithDescription("Getting game profile information")
            .WithName("Get profile info")
            .WithTags("Profiles")
            .Produces<ResponseMessage<ProfileReadInfoDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest);

        app.MapPost("/api/v1/profiles/details", ProfileHandler.GetProfileDetails)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting game profile information for the control panel";
                return generatedOperation;
            })
            .WithDescription("Getting game profile information for the control panel")
            .WithName("Get profile details")
            .WithTags("Profiles")
            .Produces<ResponseMessage<ProfileReadInfoDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/profiles/compile", ProfileHandler.CompileProfile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Compiling a game profile";
                return generatedOperation;
            })
            .WithDescription("Compiling a game profile")
            .WithName("Compile profile")
            .WithTags("Profiles")
            .Produces<ResponseMessage<ProfileReadInfoDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Players

        app.MapGet("/api/v1/players", PlayersHandler.GetPlayers)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of players";
                return generatedOperation;
            })
            .WithDescription("Getting the list of players")
            .WithName("Players list")
            .WithTags("Players")
            .Produces<ResponseMessage<List<IUser>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/players/ban", PlayersHandler.BanPlayer)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Blocking a list the player";
                return generatedOperation;
            })
            .WithDescription("Blocking a list the player")
            .WithName("Ban players")
            .WithTags("Players")
            .Produces<ResponseMessage<List<IUser>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/players/remove", PlayersHandler.RemovePlayer)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting users from the player list";
                return generatedOperation;
            })
            .WithDescription("Deleting users from the player list")
            .WithName("Remove players")
            .WithTags("Players")
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/players/pardon", PlayersHandler.PardonPlayer)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Unblocking the player list";
                return generatedOperation;
            })
            .WithDescription("Unblocking the player list")
            .WithName("Pardon players")
            .WithTags("Players")
            .Produces<ResponseMessage<List<IUser>>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.BadRequest)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Files

        app.MapGet("/api/v1/file/{fileHash}", FileHandler.GetFile)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Receiving a file for download";
                return generatedOperation;
            })
            .WithDescription("Receiving a file for download")
            .WithName("Download file")
            .WithTags("Files")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound);

        app.MapPost("/api/v1/file/whiteList", FileHandler.AddFileWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Adding a file to the White-List";
                return generatedOperation;
            })
            .WithDescription("Adding a file to the White-List")
            .WithName("Add file to white list")
            .WithTags("Files")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/file/whiteList", FileHandler.RemoveFileWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting a file from the White-List";
                return generatedOperation;
            })
            .WithDescription("Deleting a file from the White-List")
            .WithName("Remove file from white list")
            .WithTags("Files")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/folder/whiteList", FileHandler.AddFolderWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Adding folders to the White-List";
                return generatedOperation;
            })
            .WithDescription("Adding folders to the White-List")
            .WithName("Add folder to white list")
            .WithTags("Files")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/folder/whiteList", FileHandler.RemoveFolderWhiteList)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting folders from the White-List";
                return generatedOperation;
            })
            .WithDescription("Deleting folders from the White-List")
            .WithName("Remove folder from white list")
            .WithTags("Files")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Settings

        app.MapGet("/api/v1/settings/platform", SettingsHandler.GetSettings)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the platform configuration";
                return generatedOperation;
            })
            .WithDescription("Getting the platform configuration")
            .WithName("Get settings")
            .WithTags("Settings")
            .Produces<ResponseMessage<SettingsReadDto>>()
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));


        app.MapPut("/api/v1/settings/platform", SettingsHandler.UpdateSettings)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Updating the platform configuration";
                return generatedOperation;
            })
            .WithDescription("Updating the platform configuration")
            .WithName("Update settings")
            .WithTags("Settings")
            .Produces<ResponseMessage>((int)HttpStatusCode.NotFound)
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Plugins

        app.MapPost("/api/v1/plugins/install", PluginHandler.InstallPlugin)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Installing the plugin in the system";
                return generatedOperation;
            })
            .WithDescription("Installing the plugin in the system")
            .WithName("Install plugin")
            .WithTags("Plugins")
            .RequireAuthorization(c => c.RequireRole("Admin"));


        app.MapGet("/api/v1/plugins", PluginHandler.GetInstalledPlugins)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of installed plugins";
                return generatedOperation;
            })
            .WithDescription("Getting the list of installed plugins")
            .WithName("Get installed plugin")
            .WithTags("Plugins")
            .Produces<ResponseMessage<PluginVersionReadDto[]>>()
            .RequireAuthorization(c => c.RequireRole("Admin"));


        app.MapDelete("/api/v1/plugins/{name}/{version}", PluginHandler.RemovePlugin)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Removing a plugin from the system";
                return generatedOperation;
            })
            .WithDescription("Removing a plugin from the system")
            .WithName("Remove plugin")
            .WithTags("Plugins")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Launcher

        app.MapPost("/api/v1/launcher/upload", LauncherUpdateHandler.UploadLauncherVersion)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Downloading the new version of the Launcher";
                return generatedOperation;
            })
            .WithDescription("Downloading the new version of the Launcher")
            .WithName("Upload launcher version")
            .WithTags("Launcher")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/launcher", LauncherUpdateHandler.GetActualVersion)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting an up-to-date version of the Launcher";
                return generatedOperation;
            })
            .WithDescription("Getting an up-to-date version of the Launcher")
            .WithName("Get actual launcher version")
            .WithTags("Launcher");

        app.MapGet("/api/v1/launcher/builds", LauncherUpdateHandler.GetBuilds)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of builds";
                return generatedOperation;
            })
            .WithDescription("Getting the list of builds")
            .WithName("Get launcher builds")
            .WithTags("Launcher")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapGet("/api/v1/launcher/platforms", LauncherUpdateHandler.GetPlatforms)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of platforms to build";
                return generatedOperation;
            })
            .WithDescription("Getting the list of platforms to build")
            .WithName("Get launcher platforms")
            .WithTags("Launcher")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Servers

        app.MapGet("/api/v1/servers/{profileName}", ServersHandler.GetServers)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting a list of servers from a profile";
                return generatedOperation;
            })
            .WithDescription("Getting a list of servers from a profile")
            .WithName("Get profile game servers")
            .WithTags("MinecraftServers")
            .Produces<ResponseMessage<List<ServerReadDto>>>()
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapPost("/api/v1/servers/{profileName}", ServersHandler.CreateServer)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Creating a server for a profile";
                return generatedOperation;
            })
            .WithDescription("Creating a server for a profile")
            .WithName("Create server to game profile")
            .WithTags("MinecraftServers")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/servers/{profileName}/{serverNamesString}", ServersHandler.RemoveServer)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting a server in the game profile";
                return generatedOperation;
            })
            .WithDescription("Deleting a server in the game profile")
            .WithName("Remove server from game profile")
            .WithTags("MinecraftServers")
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        #region Notifications

        app.MapGet("/api/v1/notifications", NotificationHandler.GetNotifications)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Getting the list of notifications";
                return generatedOperation;
            })
            .WithDescription("Getting the list of notifications")
            .WithName("Get profile notifications")
            .WithTags("Notifications")
            .Produces<ResponseMessage<List<INotification>>>()
            .RequireAuthorization(c => c.RequireRole("Admin"));

        app.MapDelete("/api/v1/notifications", NotificationHandler.ClearNotification)
            .WithOpenApi(generatedOperation => {
                generatedOperation.Summary = "Deleting all notifications";
                return generatedOperation;
            })
            .WithDescription("Deleting all notifications")
            .WithName("Delete all notifications")
            .WithTags("Notifications")
            .Produces<ResponseMessage>()
            .RequireAuthorization(c => c.RequireRole("Admin"));

        #endregion

        return app;
    }
}