using System.Collections.Frozen;
using System.Collections.Frozen;
using System.Collections.Frozen;
using Akvila.Web.Api.Dto.Files;
using AutoMapper;
using FluentValidation;
using AkvilaCore.Interfaces;

namespace Akvila.Web.Api.Core.Handlers;

public interface IFileHandler {
    static abstract Task GetFile(
        HttpContext context,
        IAkvilaManager manager,
        string fileHash);

    static abstract Task<IResult> AddFileWhiteList(
        IAkvilaManager manager,
        IValidator<List<FileWhiteListDto>> validator,
        List<FileWhiteListDto> fileDto);

    static abstract Task<IResult> RemoveFileWhiteList(
        IAkvilaManager manager,
        IValidator<List<FileWhiteListDto>> validator,
        List<FileWhiteListDto> fileDto);

    static abstract Task<IResult> AddFolderWhiteList(
        IAkvilaManager manager,
        IMapper mapper,
        IValidator<List<FolderWhiteListDto>> validator,
        List<FolderWhiteListDto> folderDto);

    static abstract Task<IResult> RemoveFolderWhiteList(
        IAkvilaManager manager,
        IMapper mapper,
        IValidator<List<FolderWhiteListDto>> validator,
        List<FolderWhiteListDto> folderDto);
}