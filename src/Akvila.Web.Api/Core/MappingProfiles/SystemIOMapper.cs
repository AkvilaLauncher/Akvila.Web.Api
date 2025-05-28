using Akvila.Web.Api.Dto.Files;
using AutoMapper;
using Akvila.Models.System;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class SystemIOMapper : Profile {
    public SystemIOMapper() {
        CreateMap<LocalFileInfo, ProfileFileReadDto>();
        CreateMap<LocalFolderInfo, ProfileFolderReadDto>();
        CreateMap<FolderWhiteListDto, LocalFolderInfo>();
    }
}