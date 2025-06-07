using System.Net;
using AutoMapper;
using FluentValidation;
using Akvila.Web.Api.Core.Extensions;
using Akvila.Web.Api.Core.Integrations.Auth;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Player;
using Akvila.Web.Api.Dto.User;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using AkvilaCore.Interfaces.User;
using Microsoft.AspNetCore.Authorization;

namespace Akvila.Web.Api.Core.Handlers;

public class AuthIntegrationHandler : IAuthIntegrationHandler {
    private static async Task<IResult?> HandleCommonAuthValidation(
        HttpContext context,
        IAkvilaManager akvilaManager,
        AuthType authType)
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
            return Results.BadRequest(ResponseMessage.Create(
                "Failed to identify the device from which the authorization occurred",
                HttpStatusCode.BadRequest));

        return null; // Successful validation
    }

    private static async Task<IResult> HandleAuthenticatedUser(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IUser player,
        string userAgent)
    {
        if (player.IsBanned)
        {
            return Results.BadRequest(ResponseMessage.Create(
                "User blocked!",
                HttpStatusCode.BadRequest));
        }

        await akvilaManager.Profiles.CreateUserSessionAsync(null, player);

        if (string.IsNullOrEmpty(player.TextureSkinUrl))
        {
            player.TextureSkinUrl = (await akvilaManager.Integrations.GetSkinServiceAsync())
                .Replace("{userName}", player.Name)
                .Replace("{userUuid}", player.Uuid);
        }

        return Results.Ok(ResponseMessage.Create(
            mapper.Map<PlayerReadDto>(player),
            string.Empty,
            HttpStatusCode.OK));
    }

    private static IResult HandleAuthException(Exception exception, bool isHttpRequestException)
    {
        Console.WriteLine(exception);

        var errorMessage = string.Join('.',
            "Произошла ошибка при обмене данных с сервисом авторизации",
            exception.Message);

        return Results.BadRequest(ResponseMessage.Create(
            errorMessage,
            HttpStatusCode.InternalServerError));
    }

    public static async Task<IResult> Auth(
        HttpContext context,
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IValidator<BaseUserPassword> validator,
        IAuthService authService,
        BaseUserPassword authDto) {
        try {
            var result = await validator.ValidateAsync(authDto);
            if (!result.IsValid)
                return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                    HttpStatusCode.BadRequest));

            var authType = await akvilaManager.Integrations.GetAuthType();

            var validationResult = await HandleCommonAuthValidation(context, akvilaManager, authType);

            if (validationResult is not null)
                return validationResult;

            if (authType is not AuthType.Any && string.IsNullOrEmpty(authDto.Password)) {
                return Results.BadRequest(ResponseMessage.Create(
                    "No password specified during authorization!",
                    HttpStatusCode.BadRequest));
            }

            var authResult = await authService.CheckAuth(authDto.Login, authDto.Password, authType);

            if (!authResult.IsSuccess)
                return Results.BadRequest(ResponseMessage.Create(
                    authResult.Message ?? "Invalid login or password",
                    HttpStatusCode.Unauthorized));

            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var player = await akvilaManager.Users.GetAuthData(
                authResult.Login ?? authDto.Login,
                authDto.Password,
                userAgent,
                context.Request.Protocol,
                context.ParseRemoteAddress(),
                authResult.Uuid,
                context.Request.Headers["X-HWID"]);

            return await HandleAuthenticatedUser(akvilaManager, mapper, player, userAgent);

        }
        catch (HttpRequestException exception) {
            return HandleAuthException(exception, true);
        } catch (Exception exception) {
            return HandleAuthException(exception, false);
        }
    }

    public static async Task<IResult> AuthWithToken(
        HttpContext context,
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IAuthService authService,
        BaseUserPassword authDto) {
        try {
            var authType = await akvilaManager.Integrations.GetAuthType();
            
            var validationResult = await HandleCommonAuthValidation(context, akvilaManager, authType);
            if (validationResult != null)
                return validationResult;

            if (authType is not AuthType.Any && string.IsNullOrEmpty(authDto.AccessToken)) {
                return Results.BadRequest(ResponseMessage.Create(
                    "No AccessToken was passed",
                    HttpStatusCode.BadRequest));
            }

            var user = await akvilaManager.Users.GetUserByAccessToken(authDto.AccessToken);
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            if (user is not null && user.ExpiredDate > DateTime.Now) {
                return await HandleAuthenticatedUser(akvilaManager, mapper, user, userAgent);
            }

            return Results.BadRequest(ResponseMessage.Create(
                "Invalid login or password",
                HttpStatusCode.Unauthorized));
        } catch (HttpRequestException exception) {
            return HandleAuthException(exception, true);
        } catch (Exception exception) {
            return HandleAuthException(exception, false);
        }
    }

    [Authorize]
    public static async Task<IResult> GetIntegrationServices(IAkvilaManager akvilaManager, IMapper mapper) {
        var authServices = await akvilaManager.Integrations.GetAuthServices();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<AuthServiceReadDto>>(authServices), string.Empty,
            HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> GetAuthService(IAkvilaManager akvilaManager, IMapper mapper) {
        var activeAuthService = await akvilaManager.Integrations.GetActiveAuthService();

        return activeAuthService == null
            ? Results.NotFound(ResponseMessage.Create("The service for authorization is not configured", HttpStatusCode.NotFound))
            : Results.Ok(ResponseMessage.Create(mapper.Map<AuthServiceReadDto>(activeAuthService), string.Empty,
                HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> SetAuthService(
        IAkvilaManager akvilaManager,
        IValidator<IntegrationUpdateDto> validator,
        IntegrationUpdateDto updateDto) {
        var result = await validator.ValidateAsync(updateDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        var service = await akvilaManager.Integrations.GetAuthService(updateDto.AuthType);

        if (service == null)
            service = (await akvilaManager.Integrations.GetAuthServices()).FirstOrDefault(c =>
                c.AuthType == updateDto.AuthType);

        if (service == null) return Results.NotFound();

        service.Endpoint = updateDto.Endpoint;

        await akvilaManager.Integrations.SetActiveAuthService(service);

        return Results.Ok(ResponseMessage.Create("The authorization service has been successfully updated", HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> RemoveAuthService(IAkvilaManager akvilaManager) {
        await akvilaManager.Integrations.SetActiveAuthService(null);

        return Results.Ok(ResponseMessage.Create("The authorization service has been successfully removed", HttpStatusCode.OK));
    }
}