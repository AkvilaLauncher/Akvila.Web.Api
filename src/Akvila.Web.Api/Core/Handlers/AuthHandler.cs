using System.Net;
using Akvila.Web.Api.Core.Repositories;
using Akvila.Web.Api.Data;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Player;
using Akvila.Web.Api.Dto.User;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public class AuthHandler : IAuthHandler {
    public static async Task<IResult> CreateUser(
        IUserRepository userRepository,
        IValidator<UserCreateDto> validator,
        IMapper mapper,
        UserCreateDto createDto,
        ApplicationContext appContext) {
        if (appContext.Settings.RegistrationIsEnabled == false)
            return Results.BadRequest(ResponseMessage.Create("Registration for new users is prohibited",
                HttpStatusCode.BadRequest));

        var result = await validator.ValidateAsync(createDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        var user = await userRepository.CheckExistUser(createDto.Login, createDto.Email);

        if (user is not null)
            return Results.BadRequest(ResponseMessage.Create("A user with the specified data already exists",
                HttpStatusCode.BadRequest));

        user = await userRepository.CreateUser(createDto.Email, createDto.Login, createDto.Password);

        return Results.Ok(ResponseMessage.Create(mapper.Map<UserAuthReadDto>(user), "Successful registration",
            HttpStatusCode.OK));
    }

    public static async Task<IResult> UserInfo(IAkvilaManager manager, IMapper mapper, string userName) {
        var user = await manager.Users.GetUserByName(userName);

        if (user is null) {
            return Results.NotFound(ResponseMessage.Create("User not found", HttpStatusCode.BadRequest));
        }

        return Results.Ok(ResponseMessage.Create(mapper.Map<PlayerTextureDto>(user), "Successful authorization",
            HttpStatusCode.OK));
    }

    public static async Task<IResult> AuthUser(
        IUserRepository userRepository,
        IValidator<UserAuthDto> validator,
        IMapper mapper,
        UserAuthDto authDto) {
        var result = await validator.ValidateAsync(authDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        var user = await userRepository.GetUser(authDto.Login, authDto.Password);

        if (user is null)
            return Results.BadRequest(ResponseMessage.Create("Invalid login or password",
                HttpStatusCode.BadRequest));

        return Results.Ok(ResponseMessage.Create(mapper.Map<UserAuthReadDto>(user), "Successful authorization",
            HttpStatusCode.OK));
    }

    public static Task<IResult> UpdateUser(IUserRepository userRepository, UserUpdateDto userUpdateDto) {
        throw new NotImplementedException();
    }
}