using System.Net;
using Akvila.Web.Api.Core.Integrations.Auth;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Player;
using Akvila.Web.Api.Dto.User;
using AutoMapper;
using FluentValidation;
using Akvila.Web.Api.Core.Extensions;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Akvila.Web.Api.Core.Handlers;

public class AuthIntegrationHandler : IAuthIntegrationHandler {
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
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrWhiteSpace(userAgent))
                return Results.BadRequest(ResponseMessage.Create(
                    "Failed to identify the device from which the authorization occurred",
                    HttpStatusCode.BadRequest));

            if (authType is not AuthType.Any && string.IsNullOrEmpty(authDto.Password)) {
                return Results.BadRequest(ResponseMessage.Create(
                    "No password specified during authorization!",
                    HttpStatusCode.BadRequest));
            }

            var authResult = await authService.CheckAuth(authDto.Login, authDto.Password, authType);

            if (authResult.IsSuccess) {
                var player = await akvilaManager.Users.GetAuthData(
                    authResult.Login ?? authDto.Login,
                    authDto.Password,
                    userAgent,
                    context.Request.Protocol,
                    context.ParseRemoteAddress(),
                    authResult.Uuid,
                    context.Request.Headers["X-HWID"]);

                if (player.IsBanned) {
                    return Results.BadRequest(ResponseMessage.Create(
                        "User blocked!",
                        HttpStatusCode.BadRequest));
                }

                await akvilaManager.Profiles.CreateUserSessionAsync(null, player);

                player.TextureSkinUrl ??= (await akvilaManager.Integrations.GetSkinServiceAsync())
                    .Replace("{userName}", player.Name)
                    .Replace("{userUuid}", player.Uuid);

                return Results.Ok(ResponseMessage.Create(
                    mapper.Map<PlayerReadDto>(player),
                    string.Empty,
                    HttpStatusCode.OK));
            }

            return Results.BadRequest(ResponseMessage.Create(authResult.Message ?? "Invalid login or password",
                HttpStatusCode.Unauthorized));
        } catch (HttpRequestException exception) {
            Console.WriteLine(exception);
            return Results.BadRequest(ResponseMessage.Create(
                "An error occurred when exchanging data with the authorization service.", HttpStatusCode.InternalServerError));
        } catch (Exception exception) {
            Console.WriteLine(exception);
            return Results.BadRequest(ResponseMessage.Create(exception.Message, HttpStatusCode.InternalServerError));
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
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrWhiteSpace(userAgent))
                return Results.BadRequest(ResponseMessage.Create(
                    "Failed to identify the device from which the authorization occurred",
                    HttpStatusCode.BadRequest));

            if (authType is not AuthType.Any && string.IsNullOrEmpty(authDto.AccessToken)) {
                return Results.BadRequest(ResponseMessage.Create(
                    "No AccessToken was passed",
                    HttpStatusCode.BadRequest));
            }

            var user = await akvilaManager.Users.GetUserByAccessToken(authDto.AccessToken);

            if (user is not null && user.ExpiredDate > DateTime.Now) {
                var player = user;

                if (player.IsBanned) {
                    return Results.BadRequest(ResponseMessage.Create(
                        "User blocked!",
                        HttpStatusCode.BadRequest));
                }

                if (string.IsNullOrEmpty(player.TextureSkinUrl))
                    player.TextureSkinUrl = (await akvilaManager.Integrations.GetSkinServiceAsync())
                        .Replace("{userName}", player.Name)
                        .Replace("{userUuid}", player.Uuid);

                await akvilaManager.Profiles.CreateUserSessionAsync(null, player);

                return Results.Ok(ResponseMessage.Create(
                    mapper.Map<PlayerReadDto>(player),
                    string.Empty,
                    HttpStatusCode.OK));
            }
        } catch (HttpRequestException exception) {
            Console.WriteLine(exception);
            return Results.BadRequest(ResponseMessage.Create(
                "An error occurred when exchanging data with the authorization service.", HttpStatusCode.InternalServerError));
        } catch (Exception exception) {
            Console.WriteLine(exception);
            return Results.BadRequest(ResponseMessage.Create(exception.Message, HttpStatusCode.InternalServerError));
        }

        return Results.BadRequest(ResponseMessage.Create("Invalid login or password", HttpStatusCode.Unauthorized));
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