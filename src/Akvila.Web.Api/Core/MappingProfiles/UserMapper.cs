using Akvila.Web.Api.Domains.User;
using Akvila.Web.Api.Dto.User;
using AutoMapper;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class UserMapper : Profile {
    public UserMapper() {
        CreateMap<User, UserAuthReadDto>();
    }
}