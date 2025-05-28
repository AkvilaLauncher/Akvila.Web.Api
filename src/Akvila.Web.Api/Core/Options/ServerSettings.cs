using Newtonsoft.Json;

namespace Akvila.Web.Api.Core.Options;

public class ServerSettings {
    [JsonProperty(nameof(PolicyName))] public string PolicyName { get; set; } = "AkvilaPolicy";
    [JsonProperty(nameof(ProjectName))] public string ProjectName { get; set; } = "AkvilaServer";
    [JsonProperty(nameof(SecurityKey))] public string SecurityKey { get; set; } = "SecretAkvilaKey";
    public string ProjectVersion { get; set; } = null!;
    public string[] SkinDomains { get; set; } = [];

    [JsonProperty(nameof(ProjectDescription))]
    public string? ProjectDescription { get; set; }

    public string? ProjectPath { get; set; }
    public string? TextureEndpoint { get; set; }
}