using Akvila.Web.Api.Domains.Integrations;
using Akvila.Web.Api.Dto.Integration;
using AutoMapper;
using AkvilaCore.Interfaces.Integrations;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class DiscordRpcMapper : Profile {
    public DiscordRpcMapper() {
        CreateMap<DiscordRpcUpdateDto, DiscordRpcClient>();
        CreateMap<IDiscordRpcClient, DiscordRpcReadDto>();
    }
}