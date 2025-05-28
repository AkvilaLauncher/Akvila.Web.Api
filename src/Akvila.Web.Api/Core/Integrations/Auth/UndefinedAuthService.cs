using System.Text;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;
using Newtonsoft.Json;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class UndefinedAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : IPlatformAuthService {
    private readonly IAkvilaManager _akvilaManager = akvilaManager;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<AuthResult> Auth(string login, string password) {
        var activeAuthService = await _akvilaManager.Integrations.GetActiveAuthService();

        if (activeAuthService == null) throw new Exception("Authorization service is not configured or is configured incorrectly");

        var dto = JsonConvert.SerializeObject(new {
            Login = login,
            Password = password
        });

        var content = new StringContent(dto, Encoding.UTF8, "application/json");

        var result = await _httpClient.PostAsync(activeAuthService.Endpoint, content);

        return new AuthResult {
            Login = login,
            IsSuccess = result.IsSuccessStatusCode
        };
    }
}