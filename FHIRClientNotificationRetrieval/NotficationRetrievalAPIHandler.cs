using FHIRClientNotificationRetrieval.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientNotificationRetrieval
{
    public class NotficationRetrievalAPIHandler
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<NotficationRetrievalAPIHandler> _logger;

        public NotficationRetrievalAPIHandler(HttpClient httpClient, ILogger<NotficationRetrievalAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<OneOf<NotificationRetrievalSuccessResponse?, NotificationRetrievalFailedResponse?>> GetNotificationsAsync(
            string token,
            NotificationRetrievalAPI notificationRetrievalAPI)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(notificationRetrievalAPI.Accept!);



            _logger.LogInformation($"Request: {JsonUtil.ToJson(notificationRetrievalAPI)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(notificationRetrievalAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                Dictionary<string, string>? queryParams = new Dictionary<string, string>();
                notificationRetrievalAPI.RequestParameters!.ForEach(param =>
                {
                    if (param.Inject == true)
                    {
                        queryParams.Add(param.Name!, param.Value!);
                    }
                });

                var responseMessage = await httpClient.GetAsync(url: notificationRetrievalAPI.EndpointURL!, queryParams: queryParams);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<NotificationRetrievalFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }

                var response = JsonUtil.FromJson<NotificationRetrievalSuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {notificationRetrievalAPI.HttpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }

    }
}
