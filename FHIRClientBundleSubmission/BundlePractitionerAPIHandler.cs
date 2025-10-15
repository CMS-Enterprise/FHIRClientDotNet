using FHIRClientBundleSubmission.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientBundlePractitioner
{
    public class BundlePractitionerAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BundlePractitionerAPIHandler> _logger;

        public BundlePractitionerAPIHandler(HttpClient httpClient, ILogger<BundlePractitionerAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OneOf<BundlePractitionerAPISuccessResponse?, BundlePractitionerAPIFailedResponse?>> SubmitBundlePractitionerAsync(
          string token,
          BundlePractitionerAPI bundlePractitionerAPI,
          string ? guidId = null)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(bundlePractitionerAPI.Accept!);


            guidId = string.IsNullOrEmpty(guidId) ? GuidUtility.GenerateGuid() : guidId;

            string bundlePractitionerDataStr = JsonUtil.ToJson(bundlePractitionerAPI);

            bundlePractitionerDataStr = bundlePractitionerDataStr.Replace("${PractitionerIdValue}", guidId);

            var bundlePractitionerDataUpdated = JsonUtil.FromJson<BundlePractitionerAPI>(bundlePractitionerDataStr);

            _logger.LogInformation($"Request: {bundlePractitionerDataStr}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(bundlePractitionerAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                var responseMessage = await httpClient.PostAsync(bundlePractitionerAPI.EndpointURL!, bundlePractitionerDataUpdated?.Request, bundlePractitionerAPI.ContentType!);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<BundlePractitionerAPIFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }

                var response = JsonUtil.FromJson<BundlePractitionerAPISuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {(bundlePractitionerAPI.HttpClientRequestTimeOutSeconds)}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }

    }
}
