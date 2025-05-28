using System.Net;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Texture;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public class SentryErrorSaveHandler : IErrorSaveHandler {
    public static async Task<IResult> GetDsnUrl(HttpContext context, IAkvilaManager akvilaManager) {
        var serviceUrl = await akvilaManager.Integrations.GetSentryService() ??
                         $"{context.Request.Scheme}://akvila@{context.Request.Host.Value}/1";

        return Results.Ok(ResponseMessage.Create(new UrlServiceDto(serviceUrl), "Successfully", HttpStatusCode.OK));
    }

    public static async Task<IResult> UpdateDsnUrl(HttpContext context, IAkvilaManager akvilaManager, IMapper mapper,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto) {
        var result = await validator.ValidateAsync(urlDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        await akvilaManager.Integrations.SetSentryService(urlDto.Url);

        return Results.Ok(ResponseMessage.Create("Sentry service has been successfully upgraded", HttpStatusCode.OK));
    }
}