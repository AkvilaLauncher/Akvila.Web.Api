using Akvila.Web.Api.Dto.Profile;
using Akvila.Web.Api.Dto.Servers;
using AutoMapper;
using Akvila.Web.Api.Dto.Files;
using Akvila.Core.Launcher;
using Akvila.Models;
using Akvila.Models.Servers;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class ProfileMapper : Profile {
    public ProfileMapper() {
        CreateMap<MinecraftServer, ServerReadDto>();
        CreateMap<GameProfile, ProfileReadDto>();
        CreateMap<GameProfileInfo, ProfileReadInfoDto>();
    }
}