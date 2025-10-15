using FHIRClientBundleSubmission.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;
namespace FHIRClientBundleSubmission
{

    public class BundleSubmissionAPIHandler
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<BundleSubmissionAPIHandler> _logger;

        public BundleSubmissionAPIHandler(HttpClient httpClient, ILogger<BundleSubmissionAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<OneOf<BundleSubmissionAPISuccessResponse?, BundleSubmissionAPIFailedResponse?>> SubmitBundleAsync(
            string url,
            string token,
            SubmissionBundleAPIRequest request,
            string headerAccept = "application/fhir+json",
            string contentType = "application/fhir+json",
            double httpClientRequestTimeOutSeconds = 2.0,
            string guidId="")
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(headerAccept);

            request.Id = string.IsNullOrEmpty(request.Id) && string.IsNullOrEmpty(guidId) ? GuidUtility.GenerateGuid() :
              (!string.IsNullOrEmpty(guidId) ? guidId : request.Id);

            _logger.LogInformation($"Request: {JsonUtil.ToJson(request)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                var responseMessage = await httpClient.PostAsync(url, request, contentType);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<BundleSubmissionAPIFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }
                  
                var response = JsonUtil.FromJson<BundleSubmissionAPISuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {httpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }


        public string GetMessage()
        {
            _logger.LogInformation("GetMessage was called.");
            return "Hello from BundleSubmission Library!";
        }


    }
}
