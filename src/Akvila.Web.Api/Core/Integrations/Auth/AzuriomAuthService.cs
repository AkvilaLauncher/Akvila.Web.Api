using System.Text;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;
using Newtonsoft.Json;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class AzuriomAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : IPlatformAuthService {
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<AuthResult> Auth(string login, string password) {
        var authService = (await akvilaManager.Integrations.GetActiveAuthService())!.Endpoint;

        var baseUri = new Uri(authService);

        var endpoint = $"{baseUri.Scheme}://{baseUri.Host}/api/auth/authenticate";

        var dto = JsonConvert.SerializeObject(new {
            email = login,
            password,
            code = string.Empty
        });

        var content = new StringContent(dto, Encoding.UTF8, "application/json");

        var result =
            await _httpClient.PostAsync(endpoint, content);

        var resultContent = await result.Content.ReadAsStringAsync();

        var model = JsonConvert.DeserializeObject<AzuriomAuthResult>(resultContent);

        if (!result.IsSuccessStatusCode &&
            resultContent.Contains("invalid_credentials", StringComparison.OrdinalIgnoreCase)) {
            return new AuthResult {
                IsSuccess = false,
                Message = $"Invalid login or password."
            };
        }

        if (model is null || model.Banned || !result.IsSuccessStatusCode || (!result.IsSuccessStatusCode &&
                                                                             resultContent.Contains("banned",
                                                                                 StringComparison.OrdinalIgnoreCase))) {
            return new AuthResult {
                IsSuccess = false,
                Message = $"User blocked."
            };
        }

        return new AuthResult {
            Uuid = model.Uuid,
            Login = model.Username ?? login,
            IsSuccess = result.IsSuccessStatusCode
        };
    }
}