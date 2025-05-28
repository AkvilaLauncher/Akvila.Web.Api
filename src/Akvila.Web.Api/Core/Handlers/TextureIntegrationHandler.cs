using System.Net;
using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Texture;
using FluentValidation;
using AkvilaCore.Interfaces;
using Akvila.Core.User;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;

namespace Akvila.Web.Api.Core.Handlers;

public class TextureIntegrationHandler : ITextureIntegrationHandler {
    public static async Task<IResult> GetSkinUrl(IAkvilaManager akvilaManager) {
        try {
            var url = await akvilaManager.Integrations.GetSkinServiceAsync();

            return Results.Ok(ResponseMessage.Create(new UrlServiceDto(url), "Successful", HttpStatusCode.OK));
        }
        catch (Exception exception) {
            return Results.BadRequest(ResponseMessage.Create(exception.Message, HttpStatusCode.BadRequest));
        }
    }

    public static async Task<IResult> SetSkinUrl(
        IAkvilaManager akvilaManager,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto) {
        var result = await validator.ValidateAsync(urlDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        await akvilaManager.Integrations.SetSkinServiceAsync(urlDto.Url);

        return Results.Ok(ResponseMessage.Create("The skin service has been successfully updated", HttpStatusCode.OK));
    }

    public static async Task<IResult> GetCloakUrl(IAkvilaManager akvilaManager) {
        try {
            var url = await akvilaManager.Integrations.GetCloakServiceAsync();

            return Results.Ok(ResponseMessage.Create(new UrlServiceDto(url), "Successful", HttpStatusCode.OK));
        }
        catch (Exception exception) {
            return Results.BadRequest(ResponseMessage.Create(exception.Message, HttpStatusCode.BadRequest));
        }
    }

    public static async Task<IResult> SetCloakUrl(
        IAkvilaManager akvilaManager,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto) {
        var result = await validator.ValidateAsync(urlDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        await akvilaManager.Integrations.SetCloakServiceAsync(urlDto.Url);

        return Results.Ok(ResponseMessage.Create("The cape service has been successfully updated", HttpStatusCode.OK));
    }

    public static async Task<IResult> UpdateUserSkin(
        HttpContext context,
        ISkinServiceManager skinServiceManager,
        IAkvilaManager akvilaManager) {
        var login = context.Request.Form["Login"].FirstOrDefault();
        var token = context.Request.Headers.Authorization.First()?.Split(' ').LastOrDefault();

        if (string.IsNullOrEmpty(login)) {
            return Results.BadRequest(ResponseMessage.Create("Required field not filled in \"Texture\"",
                HttpStatusCode.BadRequest));
        }

        if (await akvilaManager.Users.GetUserByName(login) is not AuthUser user
            || string.IsNullOrEmpty(token)
            || string.IsNullOrEmpty(user.AccessToken)
            || !user.AccessToken.Equals(token)) {
            return Results.NotFound(ResponseMessage.Create("Identification error",
                HttpStatusCode.NotFound));
        }

        var texture = context.Request.Form.Files["Texture"]?.OpenReadStream();

        if (texture is null) {
            return Results.BadRequest(ResponseMessage.Create("Required field not filled in \"Texture\"",
                HttpStatusCode.BadRequest));
        }

        await skinServiceManager.UpdateSkin(user, texture);

        return await skinServiceManager.UpdateCloak(user, texture)
            ? Results.Ok(ResponseMessage.Create("The skin was successfully installed!", HttpStatusCode.OK))
            : Results.BadRequest(ResponseMessage.Create("Failed to update the skin!", HttpStatusCode.BadRequest));
    }

    public static async Task<IResult> UpdateUserCloak(
        HttpContext context,
        ISkinServiceManager skinServiceManager,
        IAkvilaManager akvilaManager) {
        var login = context.Request.Form["Login"].FirstOrDefault();
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').FirstOrDefault();

        if (string.IsNullOrEmpty(login)) {
            return Results.BadRequest(ResponseMessage.Create("Required field not filled in \"Texture\"",
                HttpStatusCode.BadRequest));
        }

        if (await akvilaManager.Users.GetUserByName(login) is not AuthUser user
            || string.IsNullOrEmpty(token)
            || string.IsNullOrEmpty(user.AccessToken)
            || !user.AccessToken.Equals(token)) {
            return Results.NotFound(ResponseMessage.Create("Identification error",
                HttpStatusCode.NotFound));
        }

        var texture = context.Request.Form.Files["Texture"]?.OpenReadStream();

        if (texture is null) {
            return Results.BadRequest(ResponseMessage.Create("Required field not filled in \"Texture\"",
                HttpStatusCode.BadRequest));
        }

        return await skinServiceManager.UpdateCloak(user, texture)
            ? Results.Ok(ResponseMessage.Create("The cloak has been successfully installed!", HttpStatusCode.OK))
            : Results.BadRequest(ResponseMessage.Create("Failed to update the cloak!", HttpStatusCode.BadRequest));
    }

    public static async Task<IResult> GetUserSkin(IAkvilaManager akvilaManager, string textureGuid) {
        var user = await akvilaManager.Users.GetUserBySkinGuid(textureGuid);

        if (user is null) {
            return Results.NotFound();
        }

        return Results.File(await akvilaManager.Users.GetSkin(user));
    }

    public static async Task<IResult> GetUserCloak(IAkvilaManager akvilaManager, string textureGuid) {
        var user = await akvilaManager.Users.GetUserByCloakGuid(textureGuid);

        if (user is null) {
            return Results.NotFound();
        }

        return Results.File(await akvilaManager.Users.GetCloak(user));
    }

    public static async Task<IResult> GetUserHead(IAkvilaManager akvilaManager, string userUuid) {
        var user = await akvilaManager.Users.GetUserByUuid(userUuid);

        if (user is null)
            return Results.NotFound(ResponseMessage.Create($"The user with the UUID: \"{userUuid}\" was not found.",
                HttpStatusCode.NotFound));

        return Results.Stream(await akvilaManager.Users.GetHead(user).ConfigureAwait(false));
    }
}