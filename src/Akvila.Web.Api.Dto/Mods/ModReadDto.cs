using AkvilaCore.Interfaces.Mods;

namespace Akvila.Web.Api.Dto.Mods;

public class ModReadDto {
    public string Name { get; set; }
    public string Description { get; set; }
    public ModType Type { get; set; }
}