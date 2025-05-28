using Newtonsoft.Json;

namespace Akvila.Web.Api.Dto.Sentry;

public class SentryEventLengthDto {
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("length")] public int Length { get; set; }
}