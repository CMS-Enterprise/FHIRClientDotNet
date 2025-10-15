using FHIRClientDocumentRetrievalAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientDocumentRetrievalAPI
{
    public class DeliveryConfirmationAPIHandler
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<DeliveryConfirmationAPIHandler> _logger;

        public DeliveryConfirmationAPIHandler(HttpClient httpClient, ILogger<DeliveryConfirmationAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<OneOf<DeliveryConfirmationAPIResponse?, DeliveryConfirmationAPIFailedResponse?>> ProcessDeliveryConfirmationAsync(
          string token,
          DeliveryConfirmationAPI deliveryConfirmationAPI)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(deliveryConfirmationAPI.Accept!);



            _logger.LogInformation($"Request: {JsonUtil.ToJson(deliveryConfirmationAPI.Request)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(deliveryConfirmationAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
              
                var responseMessage = await httpClient.PostAsync(url: deliveryConfirmationAPI.EndpointURL!, deliveryConfirmationAPI.Request, deliveryConfirmationAPI.ContentType!);

              
                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<DeliveryConfirmationAPIFailedResponse>(jsonContent);
                }
                else
                {
                   
                }

                var response = JsonUtil.FromJson<DeliveryConfirmationAPIResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {deliveryConfirmationAPI.HttpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }
    }
}
