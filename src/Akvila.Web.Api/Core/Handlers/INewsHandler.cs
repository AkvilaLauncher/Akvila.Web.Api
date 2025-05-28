using Akvila.Web.Api.Dto.News;
using AutoMapper;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Launcher;

namespace Akvila.Web.Api.Core.Handlers;

public interface INewsHandler {
    static abstract Task<IResult> AddNewsListener(IAkvilaManager akvilaManager, IMapper mapper,
        NewsListenerDto newsListenerDto);

    static abstract Task<IResult> GetNewsListener(IAkvilaManager akvilaManager, IMapper mapper);
    static abstract Task<IResult> GetNews(IAkvilaManager akvilaManager, IMapper mapper);
}