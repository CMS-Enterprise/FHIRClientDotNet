using System.Net.Http.Headers;

namespace FHIRClientUtilities
{
    public class HttpClientUtil
    {
        private readonly HttpClient _httpClient;

        public HttpClientUtil(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void SetAcceptHeader(string accept = "application/json")
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        }

        public void AddHeaderKeyValues(Dictionary<string, string> headers)
        {
            headers.ToList().ForEach(header =>
            {
                _httpClient.DefaultRequestHeaders.Remove(header.Key);
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            });


        }
        public async Task<HttpResponseMessage> PostAsync<T>(string url, T payload, string contentType = "application/json")
        {
            string json = JsonUtil.ToJson(payload);
            using var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, string jsonString, string contentType = "application/json")
        {
            using var content = new StringContent(jsonString);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return await _httpClient.PostAsync(url, content);
        }


        public async Task<HttpResponseMessage> PostAsync(string url, string contentType = "application/json")
        {
            using var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PostMultipartFileUploadAsync(
            string url,
            string fullFileNameWithPath,
            string? contentMD5,
            string fileNameToUseForSend,
            string contentType = "application/xml",
            Dictionary<string, string>? queryParams = null,
            Dictionary<string, string>? headers = null)
        {



            var uriBuilder = new UriBuilder($"{url}");
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                uriBuilder.Query = query;
            }


            using var content = new MultipartFormDataContent();

            using var fileStream = File.OpenRead(fullFileNameWithPath); // opens as stream
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            fileContent.Headers.ContentLength = fileStream.Length;
            if (!string.IsNullOrEmpty(contentMD5))
            {
                fileContent.Headers.ContentMD5 = CryptoUtil.ConvertBase64StringToBytes(contentMD5);
            }

            content.Add(fileContent, "file", fileNameToUseForSend); // the field name is "file"

            // Apply headers if provided
            if (headers != null)
            {
                AddHeaderKeyValues(headers);
            }

            // Send POST request
            var response = await _httpClient.PostAsync(uriBuilder.Uri, content);
            return response;
        }

        public async Task<HttpResponseMessage> PostUploadXMLFileAsync(
          string url,
          string fullFileNameWithPath,
          string? contentMD5,
          string fileNameToUseForSend,
          string contentType = "application/xml",
          Dictionary<string, string>? queryParams = null,
          Dictionary<string, string>? headers = null)
        {


            var uriBuilder = new UriBuilder($"{url}");
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                uriBuilder.Query = query;
            }

            var xml = await File.ReadAllTextAsync(fullFileNameWithPath);
            var content = new StringContent(xml);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Headers.ContentLength = xml.Length;
            if (!string.IsNullOrEmpty(contentMD5))
            {
                content.Headers.ContentMD5 = CryptoUtil.ConvertBase64StringToBytes(contentMD5);
            }

            // Apply headers if provided
            if (headers != null)
            {
                AddHeaderKeyValues(headers);
            }

            // Send POST request
            var response = await _httpClient.PostAsync(uriBuilder.Uri, content);
            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(
         string url,
         Dictionary<string, string>? queryParams = null,
         Dictionary<string, string>? headers = null)
        {


            var uriBuilder = new UriBuilder($"{url}");
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                uriBuilder.Query = query;
            }

            // Apply headers if provided
            if (headers != null)
            {
                AddHeaderKeyValues(headers);
            }

            // Send GET request
            var response = await _httpClient.GetAsync(uriBuilder.Uri);
            return response;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string url, T payload, string contentType = "application/json")
        {
            string json = JsonUtil.ToJson(payload);
            using var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return await _httpClient.PutAsync(url, content);
        }

    }
}
