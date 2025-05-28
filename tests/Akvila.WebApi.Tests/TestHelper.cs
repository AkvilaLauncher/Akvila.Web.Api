using System.Text;
using Newtonsoft.Json;

namespace Akvila.WebApi.Tests;

public class TestHelper
{
    public static HttpContent CreateJsonObject(object body)
    {
        var content = JsonConvert.SerializeObject(body);

        return new StringContent(content, Encoding.UTF8, "application/json");
    }
}
