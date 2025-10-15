using FHIRClientDocumentRetrievalAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientDocumentRetrievalAPI
{
    public class BinaryAPIHandler
    {

       

        private readonly HttpClient _httpClient;
        private readonly ILogger<BinaryAPIHandler> _logger;

        public BinaryAPIHandler(HttpClient httpClient, ILogger<BinaryAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OneOf<BinaryAPISuccessResponse?, BinaryAPIFailedResponse?>> GetBinaryFileDataAsync(
          string token,
          string fileNameId, 
          BinaryAPI binaryAPI)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(binaryAPI.Accept!);



            _logger.LogInformation($"Request: {JsonUtil.ToJson(binaryAPI)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(binaryAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {

                string url = binaryAPI.EndpointURL!.Replace("{id}", fileNameId);

                var responseMessage = await httpClient.GetAsync(url: url);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<BinaryAPIFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }

                var response = JsonUtil.FromJson<BinaryAPISuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {binaryAPI.HttpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }

    }
}
