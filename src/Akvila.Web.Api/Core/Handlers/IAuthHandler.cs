using Akvila.Web.Api.Core.Repositories;
using Akvila.Web.Api.Data;
using Akvila.Web.Api.Dto.User;
using AutoMapper;
using FluentValidation;

namespace Akvila.Web.Api.Core.Handlers;

public interface IAuthHandler {
    static abstract Task<IResult> CreateUser(
        IUserRepository userRepository,
        IValidator<UserCreateDto> validator,
        IMapper mapper,
        UserCreateDto createDto,
        ApplicationContext appContext);

    static abstract Task<IResult> AuthUser(
        IUserRepository userRepository,
        IValidator<UserAuthDto> validator,
        IMapper mapper,
        UserAuthDto authDto);

    static abstract Task<IResult> UpdateUser(
        IUserRepository userRepository,
        UserUpdateDto userUpdateDto);
}