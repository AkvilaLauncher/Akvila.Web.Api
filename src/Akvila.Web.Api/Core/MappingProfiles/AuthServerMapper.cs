using Akvila.Web.Api.Dto.Integration;
using AutoMapper;
using Akvila.Models.Auth;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class AuthServerMapper : Profile {
    public AuthServerMapper() {
        CreateMap<AuthServiceInfo, AuthServiceReadDto>();
    }
}