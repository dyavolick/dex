using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dex_webapp.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url)
        {
            return await GetJsonAsync<T>(client, url, CancellationToken.None);
        }
    }
}
