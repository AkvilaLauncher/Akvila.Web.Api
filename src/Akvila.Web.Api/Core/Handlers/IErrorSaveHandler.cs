using Akvila.Web.Api.Dto.Texture;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface IErrorSaveHandler {
    static abstract Task<IResult> GetDsnUrl(
        HttpContext context,
        IAkvilaManager akvilaManager);

    static abstract Task<IResult> UpdateDsnUrl(
        HttpContext context,
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IValidator<UrlServiceDto> validator,
        UrlServiceDto urlDto);
}