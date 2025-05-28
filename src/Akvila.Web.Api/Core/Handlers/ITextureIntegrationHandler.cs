using Akvila.Web.Api.Dto.Texture;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface ITextureIntegrationHandler {
    static abstract Task<IResult> GetSkinUrl(IAkvilaManager akvilaManager);

    static abstract Task<IResult> SetSkinUrl(
        IAkvilaManager akvilaManager,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto
    );

    static abstract Task<IResult> GetCloakUrl(IAkvilaManager akvilaManager);

    static abstract Task<IResult> SetCloakUrl(
        IAkvilaManager akvilaManager,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto);
}