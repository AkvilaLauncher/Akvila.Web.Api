using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Dto.News;

public class NewsListenerDto {
    public string Url { get; set; }
    public NewsListenerType Type { get; set; }
}