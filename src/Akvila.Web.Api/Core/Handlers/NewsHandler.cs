using System.Net;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.Dto.News;
using AutoMapper;
using AkvilaCore.Interfaces;
using AkvilaCore.Interfaces.Enums;
using Akvila.Core.Integrations;

namespace Akvila.Web.Api.Core.Handlers;

public class NewsHandler : INewsHandler {
    public static async Task<IResult> AddNewsListener(IAkvilaManager akvilaManager, IMapper mapper,
        NewsListenerDto newsListenerDto) {
        switch (newsListenerDto.Type) {
            case NewsListenerType.Azuriom:
                await akvilaManager.Integrations.NewsProvider.AddListener(new AzuriomNewsProvider(newsListenerDto.Url));
                break;
            case NewsListenerType.UnicoreCMS:
                await akvilaManager.Integrations.NewsProvider.AddListener(new UnicoreNewsProvider(newsListenerDto.Url));
                break;
            case NewsListenerType.Custom:
                await akvilaManager.Integrations.NewsProvider.AddListener(new CustomNewsProvider(newsListenerDto.Url));
                break;
            case NewsListenerType.VK:
                await akvilaManager.Integrations.NewsProvider.AddListener(new VkNewsProvider(newsListenerDto.Url,
                    akvilaManager));
                break;
            case NewsListenerType.Telegram:
            default:
                return Results.BadRequest(ResponseMessage.Create("This news provider was not found",
                    HttpStatusCode.BadRequest));
        }

        return Results.Ok(ResponseMessage.Create("Provider was successfully added", HttpStatusCode.OK));
    }

    public static async Task<IResult> RemoveNewsListener(IAkvilaManager akvilaManager, IMapper mapper,
        NewsListenerType type) {
        await akvilaManager.Integrations.NewsProvider.RemoveListenerByType(type);

        return Results.Ok(ResponseMessage.Create("Provider has been successfully removed", HttpStatusCode.OK));
    }

    public static Task<IResult> GetNewsListener(IAkvilaManager akvilaManager, IMapper mapper) {
        throw new NotImplementedException();
    }

    public static async Task<IResult> GetNews(IAkvilaManager akvilaManager, IMapper mapper) {
        var news = await akvilaManager.Integrations.NewsProvider.GetNews();

        return Results.Ok(ResponseMessage.Create(mapper.Map<List<NewsReadDto>>(news.ToList()), "Current news",
            HttpStatusCode.OK));
    }

    public static IResult GetListeners(IAkvilaManager akvilaManager) {
        return Results.Ok(ResponseMessage.Create(akvilaManager.Integrations.NewsProvider.Providers,
            "Current news", HttpStatusCode.OK));
    }
}