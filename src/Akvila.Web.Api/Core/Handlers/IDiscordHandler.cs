using Akvila.Web.Api.Dto.Integration;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface IDiscordHandler {
    static abstract Task<IResult> GetInfo(IAkvilaManager akvilaManager, IMapper mapper);

    static abstract Task<IResult> UpdateInfo(IAkvilaManager akvilaManager, IMapper mapper,
        IValidator<DiscordRpcUpdateDto> validator, DiscordRpcUpdateDto discordRpcUpdateDto);
}