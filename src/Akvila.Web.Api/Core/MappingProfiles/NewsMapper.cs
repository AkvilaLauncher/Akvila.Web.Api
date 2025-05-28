using Akvila.Web.Api.Dto.News;
using AutoMapper;
using AkvilaCore.Interfaces.News;
using Akvila.Models.News;

namespace Akvila.Web.Api.Core.MappingProfiles;

public class NewsMapper : Profile {
    public NewsMapper() {
        CreateMap<INewsData, NewsReadDto>();
        CreateMap<NewsData, NewsReadDto>();
    }
}