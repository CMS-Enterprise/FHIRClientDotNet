using FHIRClientPractitionerAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientPractitionerAPI
{
    public class PractitionerAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PractitionerAPIHandler> _logger;

        public PractitionerAPIHandler(HttpClient httpClient, ILogger<PractitionerAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<OneOf<PractitionerAPISuccessResponse?, PractitionerAPIFailedResponse?>> ProcessPractitionerRequestAsync(
         string token,
         string practitionerId,
         PractitionerAPI practitionerSettingAPI)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(practitionerSettingAPI.Accept!);



            _logger.LogInformation($"Request: {JsonUtil.ToJson(practitionerSettingAPI.Request)}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(practitionerSettingAPI.HttpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                string url = practitionerSettingAPI.EndpointURL!.Replace("{id}", practitionerId);
                var responseMessage = await httpClient.PutAsync(url: url, practitionerSettingAPI.Request, practitionerSettingAPI.ContentType!);


                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<PractitionerAPIFailedResponse>(jsonContent);
                }
                else
                {

                }

                var response = JsonUtil.FromJson<PractitionerAPISuccessResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {practitionerSettingAPI.HttpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }



    }
}
