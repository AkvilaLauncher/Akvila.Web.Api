using Akvila.Web.Api.Dto.Player;
using AutoMapper;
using Akvila.Core.User;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class PlayerMapper : Profile {
    public PlayerMapper() {
        CreateMap<AuthUser, PlayerReadDto>();
        CreateMap<AuthUser, ExtendedPlayerReadDto>();
        CreateMap<AuthUser, PlayerTextureDto>();
        CreateMap<AuthUserHistory, AuthUserHistoryDto>();
        CreateMap<ServerJoinHistory, ServerJoinHistoryDto>();
    }
}