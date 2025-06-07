using System.Net;
using Akvila.Web.Api.Dto.Files;
using Akvila.Web.Api.Dto.Messages;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.System;
using Akvila.Models.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Akvila.Web.Api.Core.Handlers;

public class FileHandler : IFileHandler {
    public static async Task GetFile(HttpContext context, IAkvilaManager akvilaManager, string fileHash) {
        var response = context.Response;

        var (file, fileName, length) = await akvilaManager.Files.GetFileStream(fileHash);

        try {
            response.Headers.Append("Content-Disposition", $"attachment; filename={fileName}");
            response.Headers.Append("Content-Length", length.ToString());
            response.ContentType = "application/octet-stream";

            await file.CopyToAsync(response.Body);
        }
        catch (Exception exception) {
            Console.WriteLine(fileName + exception);
            akvilaManager.BugTracker.CaptureException(exception);
        }
    }

    [Authorize]
    public static async Task<IResult> AddFileWhiteList(
        IAkvilaManager manager,
        IValidator<List<FileWhiteListDto>> validator,
        [FromBody] List<FileWhiteListDto> fileDto) {
        var result = await validator.ValidateAsync(fileDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        fileDto = fileDto.DistinctBy(c => c.Directory).ToList();

        var profileNames = fileDto.GroupBy(c => c.ProfileName);

        foreach (var profileFiles in profileNames) {
            var profile = await manager.Profiles.GetProfile(profileFiles.Key);

            if (profile == null)
                return Results.NotFound(ResponseMessage.Create($"A profile named \"{profileFiles.Key}\" was not found.",
                    HttpStatusCode.NotFound));

            foreach (var fileInfo in profileFiles) {
                await manager.Profiles.AddFileToWhiteList(profile, new LocalFileInfo(fileInfo.Directory));
            }
        }

        return Results.Ok(ResponseMessage.Create($"\"{fileDto.Count}\" files have been successfully added to the White-List",
            HttpStatusCode.OK));
    }

    [Authorize]
    public static async Task<IResult> RemoveFileWhiteList(
        IAkvilaManager manager,
        IValidator<List<FileWhiteListDto>> validator,
        [FromBody] List<FileWhiteListDto> fileDto) {
        var result = await validator.ValidateAsync(fileDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        fileDto = fileDto.DistinctBy(c => c.Directory).ToList();

        var profileNames = fileDto.GroupBy(c => c.ProfileName);

        foreach (var profileFiles in profileNames) {
            var profile = await manager.Profiles.GetProfile(profileFiles.Key);

            if (profile == null)
                return Results.NotFound(ResponseMessage.Create($"A profile named \"{profileFiles.Key}\" was not found.",
                    HttpStatusCode.NotFound));

            foreach (var fileInfo in profileFiles.DistinctBy(c => c.Directory)) {
                await manager.Profiles.RemoveFileFromWhiteList(profile, new LocalFileInfo(fileInfo.Directory));
            }
        }

        return Results.Ok(ResponseMessage.Create($"\"{fileDto.Count}\" files were successfully removed from the White-List",
            HttpStatusCode.OK));
    }

    public static async Task<IResult> AddFolderWhiteList(
        IAkvilaManager manager,
        IMapper mapper,
        IValidator<List<FolderWhiteListDto>> validator,
        [FromBody] List<FolderWhiteListDto> folderDto) {
        var result = await validator.ValidateAsync(folderDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        folderDto = folderDto.DistinctBy(x => x.Path).ToList();

        var mappedFolders = mapper.Map<List<LocalFolderInfo>>(folderDto);

        var profileNames = folderDto.GroupBy(c => c.ProfileName);

        foreach (var profileFolders in profileNames) {
            var profile = await manager.Profiles.GetProfile(profileFolders.Key);

            if (profile == null)
                return Results.NotFound(ResponseMessage.Create($"A profile named \"{profileFolders.Key}\" was not found.",
                    HttpStatusCode.NotFound));

            await manager.Profiles.AddFolderToWhiteList(profile, mappedFolders);
        }

        return Results.Ok(ResponseMessage.Create($"\"{folderDto.Count}\" folders has been successfully added to the White-List",
            HttpStatusCode.OK));
    }

    public static async Task<IResult> RemoveFolderWhiteList(
        IAkvilaManager manager,
        IMapper mapper,
        IValidator<List<FolderWhiteListDto>> validator,
        [FromBody] List<FolderWhiteListDto> folderDto) {
        var result = await validator.ValidateAsync(folderDto);

        if (!result.IsValid)
            return Results.BadRequest(ResponseMessage.Create(result.Errors, "Validation error",
                HttpStatusCode.BadRequest));

        folderDto = folderDto.DistinctBy(x => x.Path).ToList();

        var mappedFolders = mapper.Map<List<LocalFolderInfo>>(folderDto);

        var profileNames = folderDto.GroupBy(c => c.ProfileName);

        foreach (var profileFolders in profileNames) {
            var profile = await manager.Profiles.GetProfile(profileFolders.Key);

            if (profile == null)
                return Results.NotFound(ResponseMessage.Create($"A profile named \"{profileFolders.Key}\" was not found.",
                    HttpStatusCode.NotFound));

            await manager.Profiles.RemoveFolderFromWhiteList(profile, mappedFolders);
        }

        return Results.Ok(ResponseMessage.Create($"\"{folderDto.Count}\" folders were successfully removed from the White-List",
            HttpStatusCode.OK));
    }
}