using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace YoloTrainingLib.Providers
{
    public class HttpProvider : IHttpProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _httpClientName;

        public HttpProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpProvider(IHttpClientFactory httpClientFactory, string httpClientName)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
        }

        private HttpClient GetClient()
        {
            return _httpClientName == null
                ? _httpClientFactory.CreateClient()
                : _httpClientFactory.CreateClient(_httpClientName);
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var client = GetClient();
            var response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest content)
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync(uri, content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse> PostMultipartAsync<TResponse>(string uri, Dictionary<string, Stream> files, Dictionary<string, string>? fields = null)
        {
            var client = GetClient();

            using var content = new MultipartFormDataContent();

            // Add regular form fields (optional)
            if (fields != null)
            {
                foreach (var kv in fields)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            // Add files
            foreach (var kv in files)
            {
                var streamContent = new StreamContent(kv.Value);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "files", kv.Key);
            }

            var response = await client.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Request failed with status {response.StatusCode}: {errorBody}"
                );
            }

            return await response.Content.ReadFromJsonAsync<TResponse>()
                   ?? throw new InvalidOperationException("Response content was null");
        }

    }
}
