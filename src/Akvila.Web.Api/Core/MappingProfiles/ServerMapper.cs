using Akvila.Web.Api.Dto.Servers;
using AutoMapper;
using Akvila.Models.Servers;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class ServerMapper : Profile {
    public ServerMapper() {
        CreateMap<CreateServerDto, MinecraftServer>();
        CreateMap<MinecraftServer, ServerReadDto>();
    }
}