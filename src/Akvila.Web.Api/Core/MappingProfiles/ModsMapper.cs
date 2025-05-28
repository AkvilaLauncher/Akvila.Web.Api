using Akvila.Web.Api.Dto.Mods;
using AutoMapper;
using AkvilaCore.Interfaces.Mods;
using Akvila.Models.Mods;
using Modrinth.Api.Models.Dto.Entities;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class ModsMapper : Profile {
    public ModsMapper() {
        CreateMap<IMod, ModReadDto>();
        CreateMap<IMod, ExtendedModReadDto>();
        CreateMap<IExternalMod, ExtendedModReadDto>();
        CreateMap<IExternalMod, ExtendedModInfoReadDto>();
        CreateMap<ModrinthModVersion, ModVersionDto>();
        CreateMap<CurseForgeModVersion, ModVersionDto>();
        CreateMap<Dependency, ModVersionDtoDependency>();
    }
}