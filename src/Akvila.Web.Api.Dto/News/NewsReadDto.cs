using System;
using AkvilaCore.Interfaces.Enums;

namespace Akvila.Web.Api.Dto.News;

public record NewsReadDto {
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTimeOffset Date { get; set; }
    public NewsListenerType Type { get; set; }
}