
using FHIRClientUploadClinicalDocumentAPI.Models;
using FHIRClientUtilities;
using Microsoft.Extensions.Logging;
using OneOf;

namespace FHIRClientUploadClinicalDocumentAPI
{
    public class UploadClinicalDocumentAPIHandler
    {


        private readonly HttpClient _httpClient;
        private readonly ILogger<UploadClinicalDocumentAPIHandler> _logger;

        public UploadClinicalDocumentAPIHandler(HttpClient httpClient, ILogger<UploadClinicalDocumentAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OneOf<UploadClinicalDocumentSuccessResponse?, UploadClinicalDocumentFailedResponse?>> UploadClinicalDocumentAsync(
             string presignedUrl,
             string token,
             string baseFileLocationFolder,
             string fileName,
             string? contentMD5,
             string headerAccept = "*/*",
             string contentType = "application/xml",
             double httpClientRequestTimeOutSeconds=2.0)
        {


            var httpClient = new HttpClientUtil(_httpClient);

            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(headerAccept);

            var fullFileNameWithPath = FileUtil.GetFullFilePath(folder: baseFileLocationFolder, fileName: fileName);

            if (string.IsNullOrEmpty(contentMD5))
            {
                contentMD5 = CryptoUtil.ComputeContentMd5String(fullFileNameWithPath);
            }

            _logger.LogInformation($"ContentMD5: {contentMD5}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(httpClientRequestTimeOutSeconds)); // Per-request timeout

            try
            {

                var responseMessage = await httpClient.PostUploadXMLFileAsync(url: presignedUrl, fullFileNameWithPath: fullFileNameWithPath, fileNameToUseForSend: fileName, contentType: contentType, contentMD5: contentMD5);

                var jsonContent = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogError($"Http Response Content: {jsonContent}");
                    return JsonUtil.FromJson<UploadClinicalDocumentFailedResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }
                   

                var response = JsonUtil.FromJson<UploadClinicalDocumentSuccessResponse>(jsonContent);

                _logger.LogInformation($"FHIRClientUploadCDAPIHandler:UploadClinicalDocumentAsync response: {response}");

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {httpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }


        }

    }
}
