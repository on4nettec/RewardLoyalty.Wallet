using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Wallet.Core.Test.Helper;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static HttpRequestMessage ConfigureBearer(this HttpRequestMessage request, string? token)
    {
        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    public static void WithJsonBody(this HttpRequestMessage request, object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonWriteOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var s = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(s))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(s, JsonReadOptions);
    }
}
