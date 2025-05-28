using Akvila.Web.Api.Dto.Launcher;
using AutoMapper;
using AkvilaCore.Interfaces.Launcher;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class LauncherMapper : Profile {
    public LauncherMapper() {
        CreateMap<ILauncherBuild, LauncherBuildReadDto>();
    }
}