using FHIRClientAuthenticationAPI.Models;
using FHIRClientUtilities;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FHIRClientAuthenticationAPI
{


    public class AuthenticationAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationAPIHandler> _logger;

        public AuthenticationAPIHandler(HttpClient httpClient, ILogger<AuthenticationAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Token?> GetTokenAsync(
            string? clientId,
            string? clientSecret,
            string? scope,
            string tokenUrl,
            double httpClientRequestTimeOutSeconds=2.0)
        {


            var headerKeyValues = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(clientId))
            {
                headerKeyValues.Add("clientid", clientId);
            }
            if (!string.IsNullOrEmpty(clientSecret))
            {
                headerKeyValues.Add("clientsecret", clientSecret);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                headerKeyValues.Add("scope", scope);
            }


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.AddHeaderKeyValues(headerKeyValues);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {
                var responseMessage = await httpClient.PostAsync(tokenUrl);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();


                _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                _logger.LogInformation($"Http Response Content: {jsonContent}");

                var token = new Token();

                if (responseMessage.StatusCode != HttpStatusCode.Forbidden)
                {
                    token = JsonUtil.FromJson<Token>(jsonContent);
                }

                token!.StatusCodeValue = (int)responseMessage.StatusCode;
                token!.StatusCodeDescription = responseMessage.StatusCode.ToString();


                if (!responseMessage.IsSuccessStatusCode)
                {

                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");

                }

                return token;
            }

           
             catch (TaskCanceledException ex) when(!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {httpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }



        }
    }


}
