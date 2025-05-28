using Newtonsoft.Json;

namespace Akvila.Web.Api.Dto.Minecraft.AuthLib;

public class SkinCape {
    [JsonProperty("url")] public string Url { get; set; }
}