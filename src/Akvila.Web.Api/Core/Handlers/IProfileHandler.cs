using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Dto.Profile;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Core.Handlers;

public interface IProfileHandler {
    static abstract Task<IResult> GetProfiles(
        HttpContext context,
        IMapper mapper,
        IAkvilaManager akvilaManager);

    static abstract Task<IResult> CreateProfile(
        HttpContext context,
        ISystemService systemService,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCreateDto> validator);

    static abstract Task<IResult> UpdateProfile(
        HttpContext context,
        ISystemService systemService,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileUpdateDto> validator);

    static abstract Task<IResult> RestoreProfile(
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileRestoreDto> validator,
        ProfileRestoreDto profileName);

    static abstract Task<IResult> CompileProfile(
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCompileDto> validator,
        ProfileCompileDto profileName);

    static abstract Task<IResult> GetProfileInfo(
        HttpContext context,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCreateInfoDto> validator,
        ProfileCreateInfoDto createInfoDto);

    static abstract Task<IResult> RemoveProfile(
        IAkvilaManager akvilaManager,
        string profileName,
        bool removeFiles);

    static abstract Task<IResult> GetMinecraftVersions(IAkvilaManager akvilaManager, string gameLoader,
        string minecraftVersion);
}