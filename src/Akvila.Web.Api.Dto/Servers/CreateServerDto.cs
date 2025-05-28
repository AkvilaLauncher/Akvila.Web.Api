namespace Akvila.Web.Api.Dto.Servers;

public class CreateServerDto {
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
}