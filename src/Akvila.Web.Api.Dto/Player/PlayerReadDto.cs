using System;

namespace Akvila.Web.Api.Dto.Player;

public class PlayerReadDto : PlayerTextureDto {
    public string Uuid { get; set; }
    public string Name { get; set; } = null!;
    public string AccessToken { get; set; }
    public DateTime ExpiredDate { get; set; }
}