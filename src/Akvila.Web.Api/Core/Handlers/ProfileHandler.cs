using System.Diagnostics;
using System.Net;
using System.Web;
using Akvila.Common;
using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.Mods;
using Akvila.Web.Api.Dto.Player;
using Akvila.Web.Api.Dto.Profile;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using AkvilaCore.Interfaces.Launcher;
using AkvilaCore.Interfaces.Mods;
using Akvila.Core;
using Akvila.Core.Launcher;
using Akvila.Core.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Akvila.Web.Api.Core.Handlers;

public class ProfileHandler : IProfileHandler {
    public static async Task<IResult> GetProfiles(
        HttpContext context,
        IMapper mapper,
        IAkvilaManager akvilaManager) {
        IEnumerable<IGameProfile> profiles = [];

        if (context.User.IsInRole("Player")) {
            var userName = context.User.Identity?.Name;

            if (string.IsNullOrEmpty(userName)) {
                return Results.BadRequest(ResponseMessage.Create("Failed to identify the user",
                                                                 HttpStatusCode.BadRequest));
            }

            var user = await akvilaManager.Users.GetUserByName(userName);

            if (user is null) {
                return Results.BadRequest(ResponseMessage.Create("Failed to identify the user",
                                                                 HttpStatusCode.BadRequest));
            }

            profiles = (await akvilaManager.Profiles.GetProfiles())
                .Where(c =>
                           c is { IsEnabled: true, UserWhiteListGuid.Count: 0 } ||
                           c.UserWhiteListGuid.Any(g => g.Equals(user.Uuid)));
        } else if (context.User.IsInRole("Admin")) {
            profiles = await akvilaManager.Profiles.GetProfiles();
        }

        var gameProfiles = profiles as IGameProfile[] ?? profiles.ToArray();

        var dtoProfiles = mapper.Map<ProfileReadDto[]>(profiles);

        foreach (var profile in dtoProfiles) {
            var originalProfile = gameProfiles.First(c => c.Name == profile.Name);
            profile.Background = $"{context.Request.Scheme}://{context.Request.Host}/api/v1/file/{originalProfile.BackgroundImageKey}";
        }

        return Results.Ok(ResponseMessage.Create(dtoProfiles.OrderByDescending(c => c.Priority), string.Empty,
                                                 HttpStatusCode.OK));
    }

    public static async Task<IResult> GetMinecraftVersions(IAkvilaManager akvilaManager, string gameLoader,
                                                           string? minecraftVersion) {
        try {
            if (!Enum.TryParse<GameLoader>(gameLoader, out var loader)) {
                return Results.BadRequest(ResponseMessage.Create("Failed to determine the type of loader",
                                                                 HttpStatusCode.BadRequest));
            }

            var versions = await akvilaManager.Profiles.GetAllowVersions(loader, minecraftVersion);

            return Results.Ok(ResponseMessage.Create(versions, "Available versions of Minecraft", HttpStatusCode.OK));
        } catch (VersionNotLoadedException versionNotLoadedException) {
            return Results.NotFound(ResponseMessage.Create(versionNotLoadedException.InnerExceptionMessage,
                                                           HttpStatusCode.NotFound));
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }


    [Authorize]
    public static async Task<IResult> CreateProfile(
        HttpContext context,
        ISystemService systemService,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCreateDto> validator) {
        try {
            if (!Enum.TryParse<GameLoader>(context.Request.Form["GameLoader"], out var gameLoader))
                return Results.BadRequest(ResponseMessage.Create("Profile loader view could not be determined",
                                                                 HttpStatusCode.BadRequest));

            var createDto = new ProfileCreateDto {
                                                     Name = context.Request.Form["Name"],
                                                     DisplayName = context.Request.Form["DisplayName"],
                                                     Description = context.Request.Form["Description"],
                                                     Version = context.Request.Form["Version"],
                                                     LoaderVersion = context.Request.Form["LoaderVersion"],
                                                     GameLoader = gameLoader
                                                 };

            var result = await validator.ValidateAsync(createDto);

            if (!result.IsValid)
                return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                                                                 HttpStatusCode.BadRequest));

            var checkProfile = await akvilaManager.Profiles.GetProfile(createDto.Name);

            if (checkProfile is not null)
                return Results.BadRequest(ResponseMessage.Create("A profile with this name already exists",
                                                                 HttpStatusCode.BadRequest));

            if (!await akvilaManager.Profiles.CanAddProfile(createDto.Name, createDto.Version, createDto.LoaderVersion,
                                                            createDto.GameLoader))
                return Results.BadRequest(ResponseMessage.Create("Unable to create a profile based on received data",
                                                                 HttpStatusCode.BadRequest));

            if (context.Request.Form.Files.FirstOrDefault() is { } formFile)
                createDto.IconBase64 = await systemService.GetBase64FromImageFile(formFile);

            var profile = await akvilaManager.Profiles.AddProfile(
                createDto.Name,
                createDto.DisplayName,
                createDto.Version,
                createDto.LoaderVersion,
                createDto.GameLoader,
                createDto.IconBase64,
                createDto.Description);

            return Results.Created($"/api/v1/profiles/{createDto.Name}",
                                   ResponseMessage.Create(mapper.Map<ProfileReadDto>(profile), "Profile successfully created",
                                                          HttpStatusCode.Created));
        } catch (Exception exception) {
            Console.WriteLine(exception);
            Debug.WriteLine(exception);

            return Results.BadRequest(ResponseMessage.Create(exception.Message,
                                                             HttpStatusCode.BadRequest));
        }
    }


    [Authorize]
    public static async Task<IResult> UpdateProfile(
        HttpContext context,
        ISystemService systemService,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileUpdateDto> validator) {
        var updateDto = new ProfileUpdateDto {
                                                 Name = context.Request.Form["name"],
                                                 DisplayName = context.Request.Form["displayName"],
                                                 Description = context.Request.Form["description"],
                                                 OriginalName = context.Request.Form["originalName"],
                                                 JvmArguments = context.Request.Form["jvmArguments"],
                                                 GameArguments = context.Request.Form["gameArguments"],
                                                 Priority = int.TryParse(context.Request.Form["priority"], out var priority) ? priority : 0,
                                                 IsEnabled = context.Request.Form["enabled"] == "true"
                                             };

        var result = await validator.ValidateAsync(updateDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                                                             HttpStatusCode.BadRequest));

        var profile = await akvilaManager.Profiles.GetProfile(updateDto.OriginalName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create("Profile not found", HttpStatusCode.NotFound));

        if (updateDto.OriginalName != updateDto.Name) {
            var profileExists = await akvilaManager.Profiles.GetProfile(updateDto.Name);

            if (profileExists != null)
                return Results.NotFound(ResponseMessage.Create("A profile with this name already exists",
                                                               HttpStatusCode.NotFound));
        }

        if (!profile.CanEdit)
            return Results.NotFound(ResponseMessage.Create(
                                        "Editing is not possible in the current state of the profile", HttpStatusCode.NotFound));
        

        var icon = context.Request.Form.Files["icon"] is null
            ? null
            : context.Request.Form.Files["icon"]!.OpenReadStream();

        var background = context.Request.Form.Files["background"] is null
            ? null
            : context.Request.Form.Files["background"]!.OpenReadStream();

        await akvilaManager.Profiles.UpdateProfile(
            profile,
            updateDto.Name,
            updateDto.DisplayName,
            icon,
            background,
            updateDto.Description,
            updateDto.IsEnabled,
            updateDto.JvmArguments,
            updateDto.GameArguments,
            updateDto.Priority);

        var newProfile = mapper.Map<ProfileReadDto>(profile);
        newProfile.Background =
            $"{context.Request.Scheme}://{context.Request.Host}/api/v1/file/{profile.BackgroundImageKey}";

        var message = $"""Profile "{updateDto.Name}" has been successfully updated""";

        await akvilaManager.Notifications.SendMessage("Profile update", message, NotificationType.Info);

        return Results.Ok(ResponseMessage.Create(newProfile, message, HttpStatusCode.OK));
    }


    [Authorize]
    public static async Task<IResult> RestoreProfile(
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileRestoreDto> validator,
        ProfileRestoreDto restoreDto) {
        var result = await validator.ValidateAsync(restoreDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                                                             HttpStatusCode.BadRequest));

        var profile = await akvilaManager.Profiles.GetProfile(restoreDto.Name);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create("Profile not found", HttpStatusCode.NotFound));

        await akvilaManager.Profiles.RestoreProfileInfo(profile.Name);

        return Results.Ok(ResponseMessage.Create("Profile successfully restored", HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> CompileProfile(
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCompileDto> validator,
        ProfileCompileDto profileDto) {
        var result = await validator.ValidateAsync(profileDto);
        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                                                             HttpStatusCode.BadRequest));

        var profile = await akvilaManager.Profiles.GetProfile(profileDto.Name);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create("Profile not found", HttpStatusCode.NotFound));

        await akvilaManager.Profiles.PackProfile(profile);

        return Results.Ok(ResponseMessage.Create("Profile successfully compiled", HttpStatusCode.OK));
    }

    public static async Task<IResult> GetProfileInfo(
        HttpContext context,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCreateInfoDto> validator,
        ProfileCreateInfoDto createInfoDto) {
        var result = await validator.ValidateAsync(createInfoDto);

        if (!result.IsValid)
            return Results.BadRequest(
                ResponseMessage.Create(result.Errors, "Validation error", HttpStatusCode.BadRequest));

        if (!Enum.TryParse(createInfoDto.OsType, out OsType osType))
            return Results.BadRequest(ResponseMessage.Create(
                                          "Failed to determine the operating system type of the profile",
                                          HttpStatusCode.BadRequest));

        var osName = SystemHelper.GetStringOsType(osType);

        var profile = await akvilaManager.Profiles.GetProfile(createInfoDto.ProfileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{createInfoDto.ProfileName}\" was not found.",
                                                           HttpStatusCode.NotFound));

        var token = context.Request.Headers["Authorization"].FirstOrDefault();

        var user = await akvilaManager.Users.GetUserByName(createInfoDto.UserName);

        if (user is null || user.AccessToken != token || user.IsBanned) {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (profile.UserWhiteListGuid.Count != 0 &&
            !profile.UserWhiteListGuid.Any(c => c.Equals(user.Uuid, StringComparison.OrdinalIgnoreCase)))
            return Results.Forbid();

        user.Manager = akvilaManager;

        var profileInfo = await akvilaManager.Profiles.GetProfileInfo(profile.Name, new StartupOptions {
                                                                                                           FullScreen = createInfoDto.IsFullScreen,
                                                                                                           ServerIp = createInfoDto.GameAddress,
                                                                                                           ServerPort = createInfoDto.GamePort,
                                                                                                           ScreenHeight = createInfoDto.WindowHeight,
                                                                                                           ScreenWidth = createInfoDto.WindowWidth,
                                                                                                           MaximumRamMb = createInfoDto.RamSize,
                                                                                                           MinimumRamMb = createInfoDto.RamSize,
                                                                                                           OsName = osName,
                                                                                                           OsArch = createInfoDto.OsArchitecture
                                                                                                       }, user);

        var profileDto = mapper.Map<ProfileReadInfoDto>(profileInfo);

        profileDto.Background =
            $"{context.Request.Scheme}://{context.Request.Host}/api/v1/file/{profile.BackgroundImageKey}";

        return Results.Ok(ResponseMessage.Create(profileDto, string.Empty, HttpStatusCode.OK));
    }

    public static async Task<IResult> GetProfileDetails(
        HttpContext context,
        IMapper mapper,
        IAkvilaManager akvilaManager,
        IValidator<ProfileCreateInfoDto> validator,
        ProfileCreateInfoDto createInfoDto) {
        var result = await validator.ValidateAsync(createInfoDto);

        if (!result.IsValid)
            return Results.BadRequest(
                ResponseMessage.Create(result.Errors, "Validation error", HttpStatusCode.BadRequest));

        if (!Enum.TryParse(createInfoDto.OsType, out OsType osType))
            return Results.BadRequest(ResponseMessage.Create(
                                          "Failed to determine the operating system type of the profile",
                                          HttpStatusCode.BadRequest));

        var osName = SystemHelper.GetStringOsType(osType);

        var profile = await akvilaManager.Profiles.GetProfile(createInfoDto.ProfileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{createInfoDto.ProfileName}\" was not found.",
                                                           HttpStatusCode.NotFound));

        var user = new AuthUser {
                                    AccessToken = new string('0', 50),
                                    Uuid = Guid.NewGuid().ToString(),
                                    Name = "GmlAdmin",
                                    Manager = akvilaManager
                                };

        var profileInfo = await akvilaManager.Profiles.GetProfileInfo(profile.Name, new StartupOptions {
                                                                                                           FullScreen = createInfoDto.IsFullScreen,
                                                                                                           ServerIp = createInfoDto.GameAddress,
                                                                                                           ServerPort = createInfoDto.GamePort,
                                                                                                           ScreenHeight = createInfoDto.WindowHeight,
                                                                                                           ScreenWidth = createInfoDto.WindowWidth,
                                                                                                           MaximumRamMb = createInfoDto.RamSize,
                                                                                                           MinimumRamMb = createInfoDto.RamSize,
                                                                                                           OsName = osName,
                                                                                                           OsArch = createInfoDto.OsArchitecture
                                                                                                       }, user);

        var whiteListPlayers = await akvilaManager.Users.GetUsers(profile.UserWhiteListGuid);

        var profileDto = mapper.Map<ProfileReadInfoDto>(profileInfo);

        profileDto.Background =
            $"{context.Request.Scheme}://{context.Request.Host}/api/v1/file/{profile.BackgroundImageKey}";
        profileDto.IsEnabled = profile.IsEnabled;
        profileDto.Priority = profile.Priority;
        profileDto.UsersWhiteList = mapper.Map<List<PlayerReadDto>>(whiteListPlayers);

        return Results.Ok(ResponseMessage.Create(profileDto, string.Empty, HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> RemoveProfile(
        IAkvilaManager akvilaManager,
        string profileNames,
        [FromQuery] bool removeFiles) {
        var profileNamesList = profileNames.Split(',');
        var notRemovedProfiles = new List<string>();

        foreach (var profileName in profileNamesList) {
            var profile = await akvilaManager.Profiles.GetProfile(profileName);

            if (profile == null || profile.State == ProfileState.Loading)
                notRemovedProfiles.Add(profileName);
            else
                await akvilaManager.Profiles.RemoveProfile(profile, removeFiles);
        }

        var message = "The operation is done";

        if (notRemovedProfiles.Any()) {
            message += ". There was a skipped deletion:";
            message += string.Join(",", notRemovedProfiles);
        } else {
            message += $""". Profiles: "{profileNames}" deleted..""";
        }

        await akvilaManager.Notifications.SendMessage("Deleting profiles", message, NotificationType.Info);

        return Results.Ok(ResponseMessage.Create(message, HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> AddPlayerToWhiteList(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        string userUuid) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        var user = await akvilaManager.Users.GetUserByUuid(userUuid);

        if (user is null)
            return Results.NotFound(ResponseMessage.Create($"The user with the UUID: \"{userUuid}\" was not found.",
                                                           HttpStatusCode.NotFound));

        if (profile.UserWhiteListGuid.Any(c => c.Equals(userUuid)))
            return Results.BadRequest(ResponseMessage.Create(
                                          $"The user with the UUID: \"{userUuid}\" is already in the white list of profile users",
                                          HttpStatusCode.BadRequest));

        profile.UserWhiteListGuid.Add(user.Uuid);
        await akvilaManager.Profiles.SaveProfiles();

        var mappedUser = mapper.Map<PlayerReadDto>(user);

        return Results.Ok(ResponseMessage.Create(mappedUser,
                                                 "User has been successfully added to the profile whitelist",
                                                 HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> GetMods(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        var mods = await profile.GetModsAsync();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ModReadDto>>(mods),
                                                 "The mod list has been successfully received",
                                                 HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> UpdateModInfo(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        ModsDetailsInfoDto detailsDto,
        IValidator<ModsDetailsInfoDto> validator) {
        var result = await validator.ValidateAsync(detailsDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                                                             HttpStatusCode.BadRequest));

        try {
            await akvilaManager.Mods.SetModDetails(detailsDto.Key, detailsDto.Title, detailsDto.Description);

            return Results.Ok(ResponseMessage.Create("Value successfully updated", HttpStatusCode.OK));
        } catch (Exception exception) {
            akvilaManager.BugTracker.CaptureException(exception);
            return Results.BadRequest(ResponseMessage.Create(
                                          $"An error occurred while trying to update the mod information",
                                          HttpStatusCode.BadRequest));
        }
    }

    [Authorize]
    public static async Task<IResult> GetModsDetails(
        IAkvilaManager akvilaManager,
        IMapper mapper) {
        return Results.Ok(ResponseMessage.Create(akvilaManager.Mods.ModsDetails, "List of mods", HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> LoadMod(
        HttpContext context,
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        bool isOptional = false) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        if (await profile.CanLoadMods() == false) {
            return Results.BadRequest(ResponseMessage.Create(
                                          $"This project \"{profileName}\" can't have mods.",
                                          HttpStatusCode.NotFound));
        }

        foreach (var formFile in context.Request.Form.Files) {
            if (Path.GetExtension(formFile.FileName) != ".jar") {
                continue;
            }

            if (isOptional)
                await profile.AddOptionalMod(formFile.FileName, formFile.OpenReadStream());
            else
                await profile.AddMod(formFile.FileName, formFile.OpenReadStream());
        }

        var mods = await profile.GetModsAsync();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ModReadDto>>(mods),
                                                 "The mod list has been successfully received",
                                                 HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> LoadByLink(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        [FromBody] string[] links,
        IHttpClientFactory httpClientFactory,
        bool isOptional = false) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        if (await profile.CanLoadMods() == false) {
            return Results.BadRequest(ResponseMessage.Create(
                                          $"This project \"{profileName}\" can't have mods.",
                                          HttpStatusCode.NotFound));
        }

        using (var client = httpClientFactory.CreateClient()) {
            foreach (var link in links) {
                var fileName = Path.GetFileName(HttpUtility.UrlDecode(link));

                if (isOptional)
                    await profile.AddOptionalMod(fileName, await client.GetStreamAsync(link));
                else
                    await profile.AddMod(fileName, await client.GetStreamAsync(link));
            }
        }

        return Results.Ok(ResponseMessage.Create("Mods successfully uploaded", HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> RemoveMod(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        string fileName) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        var mods = await profile.RemoveMod(fileName);

        if (mods) {
            return Results.Ok(ResponseMessage.Create("The mod has been successfully deleted", HttpStatusCode.OK));
        }

        return Results.BadRequest(
            ResponseMessage.Create("An error occurred while deleting a modification", HttpStatusCode.OK));
    }

    public static async Task<IResult> GetOptionalsMods(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        var mods = await profile.GetOptionalsModsAsync();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ModReadDto>>(mods),
                                                 "The mod list has been successfully received",
                                                 HttpStatusCode.OK));
    }

    public static async Task<IResult> FindMods(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        string modName,
        ModType modType,
        short offset,
        short take) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        if (await profile.CanLoadMods() == false) {
            return Results.BadRequest(ResponseMessage.Create(
                                          $"This project \"{profileName}\" can't have mods.",
                                          HttpStatusCode.NotFound));
        }

        var mods = await akvilaManager.Mods.FindModsAsync(profile.Loader, profile.GameVersion, modType, modName, take,
                                                          offset);

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<ExtendedModReadDto>>(mods),
                                                 "The mod list has been successfully received", HttpStatusCode.OK));
    }

    public static async Task<IResult> GetModInfo(
        IAkvilaManager akvilaManager,
        IMapper mapper,
        string profileName,
        ModType modType,
        string modId) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        if (await profile.CanLoadMods() == false) {
            return Results.BadRequest(ResponseMessage.Create(
                                          $"This project \"{profileName}\" can't have mods.",
                                          HttpStatusCode.NotFound));
        }

        var modInfo = await akvilaManager.Mods.GetInfo(modId, modType);

        if (modInfo is null) {
            return Results.NotFound(ResponseMessage.Create($"No mod with the specified identifier found",
                                                           HttpStatusCode.NotFound));
        }

        var versions = await akvilaManager.Mods.GetVersions(modInfo, modType, profile.Loader, profile.GameVersion);
        var externalDto = mapper.Map<ExtendedModInfoReadDto>(modInfo);
        externalDto.Versions = mapper.Map<ModVersionDto[]>(versions);

        return Results.Ok(ResponseMessage.Create(externalDto, "The mod list has been successfully received",
                                                 HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> RemovePlayerFromWhiteList(
        IAkvilaManager akvilaManager,
        string profileName,
        string userUuid) {
        var profile = await akvilaManager.Profiles.GetProfile(profileName);

        if (profile is null)
            return Results.NotFound(ResponseMessage.Create($"Profile \"{profileName}\" was not found",
                                                           HttpStatusCode.NotFound));

        var user = await akvilaManager.Users.GetUserByUuid(userUuid);

        if (user is null)
            return Results.NotFound(ResponseMessage.Create($"The user with the UUID: \"{userUuid}\" was not found.",
                                                           HttpStatusCode.NotFound));

        if (!profile.UserWhiteListGuid.Any(c => c.Equals(userUuid)))
            return Results.BadRequest(ResponseMessage.Create(
                                          $"The user with the UUID: \"{userUuid}\" is not found in the white list of profile users",
                                          HttpStatusCode.BadRequest));

        profile.UserWhiteListGuid.Remove(user.Uuid);
        await akvilaManager.Profiles.SaveProfiles();

        return Results.Ok(ResponseMessage.Create("User successfully removed from profile whitelist",
                                                 HttpStatusCode.OK));
    }
}