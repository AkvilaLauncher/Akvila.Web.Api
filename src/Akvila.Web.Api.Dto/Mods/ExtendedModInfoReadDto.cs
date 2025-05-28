using System.Collections.Generic;
using AkvilaCore.Interfaces.Mods;

namespace Akvila.Web.Api.Dto.Mods;

public class ExtendedModInfoReadDto : ExtendedModReadDto {
    public IReadOnlyCollection<ModVersionDto> Versions { get; set; }
}