using Akvila.Web.Api.Domains.Settings;
using Akvila.Web.Api.Dto.Settings;
using AutoMapper;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class SettingsMapper : Profile {
    public SettingsMapper() {
        CreateMap<SettingsUpdateDto, Settings>();
        CreateMap<Settings, SettingsReadDto>();
    }
}