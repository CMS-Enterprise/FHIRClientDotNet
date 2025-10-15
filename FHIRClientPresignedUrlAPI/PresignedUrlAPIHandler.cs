using FHIRClientPresignedUrlAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.Logging;
using OneOf;
using RequestPart = FHIRClientUtilities.Models.Part;

namespace FHIRClientPresignedUrlAPI
{

    public class PresignedUrlAPIHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PresignedUrlAPIHandler> _logger;

        public PresignedUrlAPIHandler(HttpClient httpClient, ILogger<PresignedUrlAPIHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OneOf<PresignedURLAPIResponse?, PresignedURLAPIFailureResponse?>> GetPresignedURLResponseAsync(
             string url,
             string token,
             PresignedURLAPIRequest request,
             string baseFileLocationFolder,
             string headerAccept = "application/fhir+json",
             string contentType = "application/fhir+json",
             double httpClientRequestTimeOutSeconds = 2.0,
             string guidId = "")
        {



            var httpClient = new HttpClientUtil(_httpClient);

            string fileName = string.Empty;
            RequestPart? contentMD5Part = null;
            RequestPart? fileSizePart = null;

            request.Id = string.IsNullOrEmpty(request.Id) && string.IsNullOrEmpty(guidId) ? GuidUtility.GenerateGuid() : 
                (!string.IsNullOrEmpty(guidId) ?  guidId : request.Id);

          

            foreach (var parameter in request?.Parameter!)
            {

                if (parameter.Part != null)
                {

                    foreach (var part in parameter?.Part!)
                    {

                        if (part?.Name == "filename")
                        {
                            fileName = part?.ValueString!;
                        }
                        else if (part?.Name == "content-md5")
                        {
                            contentMD5Part = part;

                        }
                        else if (part?.Name == "filesize")
                        {
                            fileSizePart = part;
                        }


                    }

                    var fullFileNameWithPath = FileUtil.GetFullFilePath(folder: baseFileLocationFolder, fileName: fileName);

                    if (contentMD5Part != null && string.IsNullOrEmpty(contentMD5Part.ValueString))
                    {
                        var value = CryptoUtil.ComputeContentMd5String(fullFileNameWithPath);
                        _logger.LogInformation($"ContentMD5 Value: {value}");
                        contentMD5Part.ValueString = value;
                    }

                    if (fileSizePart != null && string.IsNullOrEmpty(fileSizePart.ValueString))
                    {
                        fileSizePart.ValueString = FileUtil.GetFileSizeInMB(fullFileNameWithPath).ToString();
                    }

                }


            }



            httpClient.SetAuthorizationToken(token);
            httpClient.SetAcceptHeader(headerAccept);

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
                    return JsonUtil.FromJson<PresignedURLAPIFailureResponse>(jsonContent);
                }
                else
                {
                    _logger.LogInformation($"Http Response Code: {responseMessage.StatusCode}");
                    _logger.LogInformation($"Http Response Content: {jsonContent}");
                }


                var response = JsonUtil.FromJson<PresignedURLAPIResponse>(jsonContent);

                return response;
            }
            catch (TaskCanceledException ex) when (!cts.Token.IsCancellationRequested)
            {
                _logger.LogInformation($"Current Request Timeout second value is {httpClientRequestTimeOutSeconds}");
                _logger.LogError($"TaskCanceledException -  Exception: {ex.Message}");
                throw;

            }



        }

        public async Task<List<PresignedURLInfo>> GetPresignedURLInfoListAsync(
           string url,
           string token,
           PresignedURLAPIRequest request,
           string baseFileLocationFolder,
           string headerAccept = "application/fhir+json",
           string contentType = "application/fhir+json",
           double httpClientRequestTimeOutSeconds = 2.0,
           string guidId = "")
        {

            var result = await GetPresignedURLResponseAsync(url: url, token: token, request: request,
               baseFileLocationFolder: baseFileLocationFolder, headerAccept: headerAccept, contentType: contentType,
               httpClientRequestTimeOutSeconds: httpClientRequestTimeOutSeconds, guidId);

            var presignedURLInfoList = new List<PresignedURLInfo>();

            result.Switch(

                presignedURLAPIResponse =>
                {
                    foreach (var parameter in presignedURLAPIResponse?.Parameter!)
                    {

                        if (parameter.Part != null)
                        {
                            var presignedURLInfo = new PresignedURLInfo();
                            presignedURLInfoList.Add(presignedURLInfo);
                            foreach (var part in parameter?.Part!)
                            {


                                part?.PartItem!.ForEach(partitem =>
                                {
                                    if (partitem.ValueString != null)
                                    {
                                        presignedURLInfo.PartValueString = new PartValueString() { Name = partitem?.Name!, ValueString = partitem?.ValueString! };
                                    }
                                    else if (partitem.ValueUrl != null)
                                    {
                                        presignedURLInfo.PartValueUrl = new PartValueUrl() { Name = partitem?.Name!, ValueUrl = partitem?.ValueUrl! };

                                    }

                                });
                            }
                        }


                    }


                },
                presignedURLAPIFailureResponse =>
                {
                    presignedURLInfoList.Clear();
                }

            );

            return presignedURLInfoList;


        }
    }
}
