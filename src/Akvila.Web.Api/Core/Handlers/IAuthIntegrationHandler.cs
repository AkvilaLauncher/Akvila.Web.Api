using Akvila.Web.Api.Core.Integrations.Auth;
using Akvila.Web.Api.Dto.Integration;
using Akvila.Web.Api.Dto.User;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface IAuthIntegrationHandler {
    static abstract Task<IResult> Auth(
        HttpContext context,
        IAkvilaManager akvilaManager,
        IMapper mapper,
        IValidator<BaseUserPassword> validator,
        IAuthService authService,
        BaseUserPassword authDto);

    static abstract Task<IResult> GetIntegrationServices(IAkvilaManager akvilaManager, IMapper mapper);

    static abstract Task<IResult> GetAuthService(IAkvilaManager akvilaManager, IMapper mapper);

    static abstract Task<IResult> SetAuthService(
        IAkvilaManager akvilaManager,
        IValidator<IntegrationUpdateDto> validator,
        IntegrationUpdateDto updateDto);

    static abstract Task<IResult> RemoveAuthService(IAkvilaManager akvilaManager);
}