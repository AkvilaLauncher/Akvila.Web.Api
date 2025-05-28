using System.Text;
using Akvila.Web.Api.Domains.Integrations;
using AkvilaCore.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Akvila.Web.Api.Core.Integrations.Auth;

public class UnicoreCMSAuthService(IHttpClientFactory httpClientFactory, IAkvilaManager akvilaManager)
    : IPlatformAuthService {
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<AuthResult> Auth(string login, string password) {
        var authService = (await akvilaManager.Integrations.GetActiveAuthService())!.Endpoint;

        var baseUri = new Uri(authService);

        var endpoint = $"{baseUri.Scheme}://{baseUri.Host}/auth/login";

        var dto = JsonConvert.SerializeObject(new {
            username_or_email = login,
            password,
            totp = string.Empty,
            save_me = string.Empty
        });

        var content = new StringContent(dto, Encoding.UTF8, "application/json");

        var result =
            await _httpClient.PostAsync(endpoint, content);

        var responseResult = await result.Content.ReadAsStringAsync();

        var data = JsonConvert.DeserializeObject<UnicoreAuthResult>(responseResult);

        if (data is null || !result.IsSuccessStatusCode || data.User is null || data?.User?.Ban is not null) {
            if (data?.User?.Ban is { } ban) {
                return new AuthResult {
                    IsSuccess = false,
                    Message = $"User blocked. Reason: {ban.Reason}"
                };
            }

            return new AuthResult {
                IsSuccess = false,
                Message = responseResult.Contains("\"statusCode\":401")
                    ? "Invalid login or password"
                    : "An error occurred while processing data from the authorization server."
            };
        }

        return new AuthResult {
            Login = data.User.Username ?? login,
            IsSuccess = result.IsSuccessStatusCode,
            Uuid = data.User.Uuid
        };
    }
}