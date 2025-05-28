using System;

namespace Akvila.Web.Api.Dto.Player;

public record ServerJoinHistoryDto {
    public string ServerUuid { get; set; }
    public DateTime Date { get; set; }
}