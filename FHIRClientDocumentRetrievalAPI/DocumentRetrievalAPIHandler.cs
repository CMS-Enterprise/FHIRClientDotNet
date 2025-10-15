using FHIRClientDocumentRetrievalAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;
namespace FHIRClientDocumentRetrievalAPI
{
    public class DocumentRetrievalAPIHandler
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<DocumentRetrievalAPIHandler> _logger;

        public DocumentRetrievalAPIHandler(HttpClient httpClient, ILogger<DocumentRetrievalAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OneOf<DocumentRetrievalAPISuccessResponse?, DocumentRetrievalAPIFailedResponse?>> RetrieveDocumentAsync(
          string token,
          DocumentRetrievalAPI documentRetrievalAPI)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(documentRetrievalAPI.Accept!);



            _logger.LogInformation($"Request: {JsonUtil.ToJson(documentRetrievalAPI)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(documentRetrievalAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                Dictionary<string, string>? queryParams = new Dictionary<string, string>();
                documentRetrievalAPI.RequestParameters!.ForEach(param =>
                {
                    if (param.Inject == true)
                    {
                        queryParams.Add(param.Name!, param.Value!);
                    }
                });

                var responseMessage = await httpClient.GetAsync(url: documentRetrievalAPI.EndpointURL!, queryParams: queryParams);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<DocumentRetrievalAPIFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }

                var response = JsonUtil.FromJson<DocumentRetrievalAPISuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {documentRetrievalAPI.HttpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }

    }
}
