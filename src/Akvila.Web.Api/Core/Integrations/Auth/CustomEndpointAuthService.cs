using System.Text;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;
using Newtonsoft.Json;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class CustomEndpointAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : IPlatformAuthService {
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<AuthResult> Auth(string login, string password) {
        var dto = JsonConvert.SerializeObject(new {
            Login = login,
            Password = password
        });

        var content = new StringContent(dto, Encoding.UTF8, "application/json");

        var result =
            await _httpClient.PostAsync((await akvilaManager.Integrations.GetActiveAuthService())!.Endpoint, content);

        var resultContent = await result.Content.ReadAsStringAsync();

        var authResult = new AuthResult {
            Login = login,
            IsSuccess = result.IsSuccessStatusCode
        };

        if (string.IsNullOrEmpty(resultContent))
            return authResult;

        var model = JsonConvert.DeserializeObject<AuthCustomResponse>(resultContent);

        authResult.Login = model?.Login ?? login;
        authResult.Uuid = model?.UserUuid;

        return authResult;
    }
}