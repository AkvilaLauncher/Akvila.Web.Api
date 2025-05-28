using System.Net.Http.Headers;
using Akvila.Web.Api.Core.Options;
using Akvila.Core.User;

namespace Akvila.Web.Api.Core.Services;

public class SkinServiceManager(IHttpClientFactory httpClientFactory) : ISkinServiceManager {
    private HttpClient _skinServiceClient = httpClientFactory.CreateClient(HttpClientNames.SkinService);

    public async Task<bool> UpdateSkin(AuthUser authUser, Stream texture) {
        var content = new MultipartFormDataContent();

        content.Add(new StreamContent(texture) {
            Headers = {
                ContentLength = texture.Length,
                ContentType = new MediaTypeHeaderValue("image/png")
            }
        }, "file", "skin.png"); 

        var request = await _skinServiceClient.PostAsync($"/skin/{authUser.Name}", content);

        return request.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCloak(AuthUser authUser, Stream texture) {
        var content = new MultipartFormDataContent();

        content.Add(new StreamContent(texture) {
            Headers = {
                ContentLength = texture.Length,
                ContentType = new MediaTypeHeaderValue("image/png")
            }
        }, "file", "skin.png"); 

        var request = await _skinServiceClient.PostAsync($"/cloak/{authUser.Name}", content);

        return request.IsSuccessStatusCode;
    }
}

public interface ISkinServiceManager {
    Task<bool> UpdateSkin(AuthUser authUser, Stream texture);
    Task<bool> UpdateCloak(AuthUser authUser, Stream texture);
}