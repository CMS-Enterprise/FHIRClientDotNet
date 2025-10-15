using FHIRClientAuthenticationAPI;
using FHIRClientAuthenticationAPI.Models;
using FHIRClientBundlePractitioner;
using FHIRClientBundleSubmission;
using FHIRClientBundleSubmission.Models;
using FHIRClientDocumentRetrievalAPI;
using FHIRClientDocumentRetrievalAPI.Models;
using FHIRClientNotificationRetrieval;
using FHIRClientNotificationRetrieval.Models;
using FHIRClientPractitionerAPI;
using FHIRClientPractitionerAPI.Models;
using FHIRClientPresignedUrlAPI;
using FHIRClientPresignedUrlAPI.Models;
using FHIRClientUploadClinicalDocumentAPI;
using FHIRClientUploadClinicalDocumentAPI.Models;
using FHIRClientUtilities;
using FHIRClientUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using OneOf;
using System.Security.Policy;

namespace FHIRClientConsoleApp
{


    public class FHIRClientAPIRunnerManager : IDisposable
    {
        private bool disposed = false;
        private readonly IServiceCollection services = new ServiceCollection();
        private readonly AppSettings settings = SettingsUtil.GetSettings();
        private readonly string nLogConfigFileWithPath;
        private readonly IServiceProvider provider;
        private Token? token;
        private readonly string sharedGuidID = GuidUtility.GenerateGuid();
        private readonly PresignedURLAPIRequest presignedURLAPIRequest;
        private readonly SubmissionBundleAPIRequest submissionBundleAPIRequest;


        public FHIRClientAPIRunnerManager(string nlogConfigFileWithPath)
        {

            this.nLogConfigFileWithPath = nlogConfigFileWithPath;
            ConfigureServices();
            provider = services.BuildServiceProvider();
            presignedURLAPIRequest = CloningUtil.DeepClone(settings.PresignedURLAPI!.Request!);
            submissionBundleAPIRequest = CloningUtil.DeepClone(settings.BundleSubmissionAPI!.Request!);

        }

        public PresignedURLAPIRequest GetPresignedURLAPIRequest()
        {
            return presignedURLAPIRequest;


        }

        public SubmissionBundleAPIRequest GetSubmissionBundleAPIRequest()
        {
            return submissionBundleAPIRequest;


        }

        public string GetSharedGuid()
        {
            return sharedGuidID;
        }


        private void ConfigureServices()
        {
            LogManager.Setup().LoadConfigurationFromFile(nLogConfigFileWithPath);
            // Configure NLog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog();
            });

            services.AddHttpClient<AuthenticationAPIHandler>();
            services.AddHttpClient<PresignedUrlAPIHandler>();
            services.AddHttpClient<UploadClinicalDocumentAPIHandler>();
            services.AddHttpClient<BundleSubmissionAPIHandler>();
            services.AddHttpClient<NotficationRetrievalAPIHandler>();
            services.AddHttpClient<DocumentRetrievalAPIHandler>();
            services.AddHttpClient<DeliveryConfirmationAPIHandler>();
            services.AddHttpClient<PractitionerAPIHandler>();
            services.AddHttpClient<BinaryAPIHandler> ();
            services.AddHttpClient<BundlePractitionerAPIHandler>();
            

        }

        public bool isValidToken()
        {
            var result = token == null || TokenUtil.IsTokenExpired((DateTime)token.IssuedAt!, (double)token.ExpiresIn!);
            if (!result)
            {
                LogManager.GetCurrentClassLogger().Info($"Existing token not valid or expired - {JsonUtil.ToJson(token)}");
            }
            return result;
        }

        public async Task<Token> GetTokenAsync(bool alwaysGenerateNewToken = false, string? overridenScopeValue = null)
        {



            var httpService = provider.GetRequiredService<AuthenticationAPIHandler>();
            try
            {
                if (!alwaysGenerateNewToken && !isValidToken())
                {
                    LogManager.GetCurrentClassLogger().Info($"Existing token is still valid and reusing - {JsonUtil.ToJson(token)}");
                    return token!;
                }

                string scopeValue = string.IsNullOrEmpty(overridenScopeValue) ? settings.AuthenticationAPI?.Scope! : overridenScopeValue;

                token = await httpService.GetTokenAsync(clientId: settings.AuthenticationAPI?.ClientId!,
                                                          clientSecret: settings.AuthenticationAPI?.ClientSecret!,
                                                          scope: scopeValue,
                                                          tokenUrl: settings.AuthenticationAPI?.EndpointURL!,
                                                          httpClientRequestTimeOutSeconds: settings.HttpClientRequestTimeOutSeconds!);

                LogManager.GetCurrentClassLogger().Info($"New Token Generated - {JsonUtil.ToJson(token)}");

                return token!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw;
            }

        }

        public async Task<OneOf<PresignedURLAPIResponse, PresignedURLAPIFailureResponse>> GetPresignedURLAsync(Token? overriddenToken = null)
        {
            using var provider = services.BuildServiceProvider();
            var httpService = provider.GetRequiredService<PresignedUrlAPIHandler>();
            try
            {
                var request = GetPresignedURLAPIRequest();
                request.Id = GetSharedGuid();

                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;

                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.GetPresignedURLResponseAsync(url: settings.PresignedURLAPI?.EndpointURL!,
                    token: tokenResult.Value!,
                    request: request,
                    baseFileLocationFolder: settings.BaseFileLocationFolder!,
                    headerAccept: settings.PresignedURLAPI?.Accept!,
                    contentType: settings.PresignedURLAPI?.ContentType!,
                    httpClientRequestTimeOutSeconds: settings.HttpClientRequestTimeOutSeconds!);

                result.Switch(

                    presignedURLAPIResponse =>
                    {
                        LogManager.GetCurrentClassLogger().Info($"GetPresignedURLAsync Success Response: {JsonUtil.ToJson<PresignedURLAPIResponse>(presignedURLAPIResponse!)}");
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PresignedURL", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(presignedURLAPIResponse!, fileName);


                    },
                    presignedURLAPIFailureResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PresignedURL", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(presignedURLAPIFailureResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"GetPresignedURLAsync Failed Response: {JsonUtil.ToJson<PresignedURLAPIFailureResponse>(presignedURLAPIFailureResponse!)}");
                    }

                    );


                var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PresignedURL", GetSharedGuid(), "request");
                FileUtil.SaveAsJsonFile(request, fileName);

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<List<PresignedURLInfo>> GetPresignedURLInfoListAsync(Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<PresignedUrlAPIHandler>();
            try
            {

                var request = GetPresignedURLAPIRequest();
                request.Id = GetSharedGuid();

                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;

                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.GetPresignedURLInfoListAsync(url: settings.PresignedURLAPI?.EndpointURL!,
                    token: tokenResult.Value!,
                    request: request,
                    baseFileLocationFolder: settings.BaseFileLocationFolder!,
                    headerAccept: settings.PresignedURLAPI?.Accept!,
                    contentType: settings.PresignedURLAPI?.ContentType!,
                    httpClientRequestTimeOutSeconds: settings.HttpClientRequestTimeOutSeconds!);

                LogManager.GetCurrentClassLogger().Info($"GetPresignedURLInfoListAsync Response: {JsonUtil.ToJson(result!)}");

                var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PresignedURL", GetSharedGuid(), "response");
                FileUtil.SaveAsJsonFile(result!, fileName);

                fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PresignedURL", GetSharedGuid(), "request");
                FileUtil.SaveAsJsonFile(request, fileName);


                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<UploadClinicalDocumentSuccessResponse?, UploadClinicalDocumentFailedResponse?>> UploadClinicalDocumentAsync(string presignedUrl,
                                                                                                                                    string fileName, Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<UploadClinicalDocumentAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.UploadClinicalDocumentAsync(presignedUrl: presignedUrl!, token: tokenResult.Value!,
                                                                            fileName: fileName,
                                                                            baseFileLocationFolder: settings.BaseFileLocationFolder!,
                                                                            contentMD5: settings.UploadClinicalDocumentAPI?.ContentMD5!,
                                                                            httpClientRequestTimeOutSeconds: settings.HttpClientRequestTimeOutSeconds!);

                result.Switch(

                    uploadClinicalDocumentSuccessResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "uploadClinicalDocument", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(uploadClinicalDocumentSuccessResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"UploadClinicalDocumentAsync Success Response: {JsonUtil.ToJson<UploadClinicalDocumentSuccessResponse>(uploadClinicalDocumentSuccessResponse!)}");
                    },
                    uploadClinicalDocumentFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "uploadClinicalDocument", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(uploadClinicalDocumentFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"UploadClinicalDocumentAsync Failed Response: {JsonUtil.ToJson<UploadClinicalDocumentFailedResponse>(uploadClinicalDocumentFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<BundleSubmissionAPISuccessResponse?, BundleSubmissionAPIFailedResponse?>> SubmitBundleAsync(UploadClinicalDocumentSuccessResponse uploadClinicalDocumentSuccessResponse, Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<BundleSubmissionAPIHandler>();
            try
            {


                LogManager.GetCurrentClassLogger().Info($"Http Request Timeout Value - {settings.BundleSubmissionAPI!.HttpClientRequestTimeOutSeconds!}");

                PrepareBundleSubmissionRequest(uploadClinicalDocumentSuccessResponse);

                LogManager.GetCurrentClassLogger().Info($"Http Request Timeout Value - {uploadClinicalDocumentSuccessResponse.Status}");

                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                    await GetTokenAsync() : overriddenToken;


                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.SubmitBundleAsync(url: settings.BundleSubmissionAPI?.EndpointURL!,
                      token: tokenResult.Value!,
                      request: GetSubmissionBundleAPIRequest(),
                      headerAccept: settings.BundleSubmissionAPI?.Accept!, contentType: settings.BundleSubmissionAPI?.ContentType!,
                      httpClientRequestTimeOutSeconds: settings.BundleSubmissionAPI!.HttpClientRequestTimeOutSeconds!);

                result.Switch(

                    bundleSubmissionAPISuccessResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "SubmissionBundle", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(bundleSubmissionAPISuccessResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"SubmitBundleAsync Success Response: {JsonUtil.ToJson<BundleSubmissionAPISuccessResponse>(bundleSubmissionAPISuccessResponse!)}");
                    },
                    bundleSubmissionAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "SubmissionBundle", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(bundleSubmissionAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"SubmitBundleAsync Failed Response: {JsonUtil.ToJson<BundleSubmissionAPIFailedResponse>(bundleSubmissionAPIFailedResponse!)}");
                    }

                    );

                var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "SubmissionBundle", GetSharedGuid(), "request");
                FileUtil.SaveAsJsonFile(GetSubmissionBundleAPIRequest(), fileName);

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<NotificationRetrievalSuccessResponse?, NotificationRetrievalFailedResponse?>> GetNotificationsAsync(Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<NotficationRetrievalAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.GetNotificationsAsync(token: tokenResult.Value!,
                                                                    notificationRetrievalAPI: settings?.NotificationRetrievalAPI!);

                result.Switch(

                    notificationRetrievalSuccessResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "notificationRetrieval", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(notificationRetrievalSuccessResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"GetNotificationsAsync Success Response: {JsonUtil.ToJson<NotificationRetrievalSuccessResponse>(notificationRetrievalSuccessResponse!)}");
                    },
                    notificationRetrievalFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "notificationRetrieval", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(notificationRetrievalFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"GetNotificationsAsync Failed Response: {JsonUtil.ToJson<NotificationRetrievalFailedResponse>(notificationRetrievalFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<DocumentRetrievalAPISuccessResponse?, DocumentRetrievalAPIFailedResponse?>> RetrieveDocumentAsync(Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<DocumentRetrievalAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.RetrieveDocumentAsync(token: tokenResult.Value!,
                                                                    documentRetrievalAPI: settings?.DocumentRetrievalAPI!);

                result.Switch(

                    documentRetrievalSuccessResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "documentRetrieval", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(documentRetrievalSuccessResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"RetrieveDocumentAsync Success Response: {JsonUtil.ToJson<DocumentRetrievalAPISuccessResponse>(documentRetrievalSuccessResponse!)}");
                    },
                    documentRetrievalAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "documentRetrieval", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(documentRetrievalAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"RetrieveDocumentAsync Failed Response: {JsonUtil.ToJson<DocumentRetrievalAPIFailedResponse>(documentRetrievalAPIFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<DeliveryConfirmationAPIResponse?, DeliveryConfirmationAPIFailedResponse?>> ProcessDeliveryConfirmationAsync(Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<DeliveryConfirmationAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");


                settings!.DeliveryConfirmationAPI!.Request!.Id = GuidUtility.GenerateGuid();

                var result = await httpService.ProcessDeliveryConfirmationAsync(token: tokenResult.Value!,
                                                                    deliveryConfirmationAPI: settings?.DeliveryConfirmationAPI!);

                result.Switch(

                    deliveryConfirmationAPIResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "deliveryConfirmation", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(deliveryConfirmationAPIResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"ProcessDeliveryConfirmationAsync Success Response: {JsonUtil.ToJson<DeliveryConfirmationAPIResponse>(deliveryConfirmationAPIResponse!)}");
                    },
                    deliveryConfirmationAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "deliveryConfirmation", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(deliveryConfirmationAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"ProcessDeliveryConfirmationAsync Failed Response: {JsonUtil.ToJson<DeliveryConfirmationAPIFailedResponse>(deliveryConfirmationAPIFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<PractitionerAPISuccessResponse?, PractitionerAPIFailedResponse?>> ProcessPractitionerRequestAsync(Token? overriddenToken = null, string? practitionerId=null )
        {
            var httpService = provider.GetRequiredService<PractitionerAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");


                practitionerId = practitionerId ?? settings?.PractitionerAPI?.Request?.Id!;

                var result = await httpService.ProcessPractitionerRequestAsync(token: tokenResult.Value!, 
                                                                    practitionerId: practitionerId,
                                                                    practitionerSettingAPI: settings?.PractitionerAPI!);

                result.Switch(

                    practitionerAPIResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "practitioner", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(practitionerAPIResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"ProcessPractitionerRequestAsync Success Response: {JsonUtil.ToJson<PractitionerAPISuccessResponse>(practitionerAPIResponse!)}");
                    },
                    practitionerAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "practitioner", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(practitionerAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"ProcessPractitionerRequestAsync Failed Response: {JsonUtil.ToJson<PractitionerAPIFailedResponse>(practitionerAPIFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<BinaryAPISuccessResponse?, BinaryAPIFailedResponse?>> GetBinaryFileDataAsync(Token? overriddenToken = null, string? fileNameId = null)
        {
            var httpService = provider.GetRequiredService<BinaryAPIHandler>();
            try
            {


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                     await GetTokenAsync() : overriddenToken;
                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");


                fileNameId = fileNameId ?? settings?.BinaryAPI?.FileNameId!;

                var result = await httpService.GetBinaryFileDataAsync(token: tokenResult.Value!,
                                                                    fileNameId: fileNameId,
                                                                    binaryAPI: settings?.BinaryAPI!);

                result.Switch(

                    binaryAPIResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "binaryapi", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(binaryAPIResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"GetBinaryFileDataAsync Success Response: {JsonUtil.ToJson<BinaryAPISuccessResponse>(binaryAPIResponse!)}");
                    },
                    binaryAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings?.BaseFileLocationFolder!, "binaryapi", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(binaryAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"GetBinaryFileDataAsync Failed Response: {JsonUtil.ToJson<BinaryAPIFailedResponse>(binaryAPIFailedResponse!)}");
                    }

                    );

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }

        public async Task<OneOf<BundlePractitionerAPISuccessResponse?, BundlePractitionerAPIFailedResponse?>> SubmitBundlePractitionerAsync(Token? overriddenToken = null)
        {
            var httpService = provider.GetRequiredService<BundlePractitionerAPIHandler>();
            try
            {


                LogManager.GetCurrentClassLogger().Info($"Http Request Timeout Value - {settings.BundlePractitionerAPI!.HttpClientRequestTimeOutSeconds!}");


                var tokenResult = (overriddenToken == null || TokenUtil.IsTokenExpired((DateTime)overriddenToken.IssuedAt!, (double)overriddenToken.ExpiresIn!)) ?
                    await GetTokenAsync() : overriddenToken;


                LogManager.GetCurrentClassLogger().Info($"Token Value - {tokenResult.Value!}");

                var result = await httpService.SubmitBundlePractitionerAsync(token: tokenResult.Value!, bundlePractitionerAPI: settings.BundlePractitionerAPI!);

                result.Switch(

                    bundlePractitionerAPISuccessResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PractitionerBundle", GetSharedGuid(), "response-success");
                        FileUtil.SaveAsJsonFile(bundlePractitionerAPISuccessResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"SubmitBundlePractitionerAsync Success Response: {JsonUtil.ToJson<BundlePractitionerAPISuccessResponse>(bundlePractitionerAPISuccessResponse!)}");
                    },
                    bundlePractitionerAPIFailedResponse =>
                    {
                        var fileName = FileUtil.GenerateGuidFilePath(settings.BaseFileLocationFolder!, "PractitionerBundle", GetSharedGuid(), "response-failed");
                        FileUtil.SaveAsJsonFile(bundlePractitionerAPIFailedResponse!, fileName);
                        LogManager.GetCurrentClassLogger().Info($"SubmitBundlePractitionerAsync Failed Response: {JsonUtil.ToJson<BundlePractitionerAPIFailedResponse>(bundlePractitionerAPIFailedResponse!)}");
                    }

                    );

             

                return result!;

            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }

        }


        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Avoid finalizer if cleanup is done
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    LogManager.GetCurrentClassLogger().Info("Disposing service...");
                    // Dispose any managed resources here
                }

                // Always call logger shutdown once per app, ideally here if this is the last service using it
                LogManager.Shutdown();

                disposed = true;
            }
        }

        private void PrepareBundleSubmissionRequest(UploadClinicalDocumentSuccessResponse uploadClinicalDocumentSuccessResponse)
        {
            var bundleRequest = GetSubmissionBundleAPIRequest();

            bundleRequest.Id = GetSharedGuid();

            var timestamp = DateTimeUtil.GetCurrentWithOffset();

            bundleRequest.Timestamp = timestamp;

            var fullFileNameWithPath = FileUtil.GetFullFilePath(folder: settings.BaseFileLocationFolder!, fileName: uploadClinicalDocumentSuccessResponse.Filename);

            var resourceTypeDocumentReferenceGuid = GuidUtility.GenerateGuid();

            var resourceTypeListGuid = GuidUtility.GenerateGuid();

            foreach (var entry in bundleRequest.Entry!)
            {



                if (entry.Resource != null)
                {
                    var resource = entry.Resource;
                    resource.Date = timestamp;

                    if (resource.ResourceType == "DocumentReference")
                    {
                        entry.FullUrl = RequestTransformerUtil.UrnUuidFormattedValue(resourceTypeDocumentReferenceGuid);

                        if (resource.Content != null && resource.Content.Count > 0)
                        {
                            resource.Id = resourceTypeDocumentReferenceGuid;
                            var attachment = resource.Content[0].Attachment;
                            if (attachment != null)
                            {
                                attachment.Id = resourceTypeDocumentReferenceGuid + "_Document";
                                attachment.Title = resourceTypeDocumentReferenceGuid + "_pkpadmin";
                                attachment.Url = uploadClinicalDocumentSuccessResponse.S3uri;
                                attachment.ContentType = "application/xml";
                                attachment.Size = FileUtil.GetFileSizeBytes(fullFileNameWithPath);
                                attachment.Hash = CryptoUtil.ComputeSHA256Checksum(fullFileNameWithPath);
                                attachment.Creation = DateTimeUtil.GetCurrentUtc();
                            }
                        }
                        if (resource.Identifier != null && resource.Identifier.Count > 0)
                        {
                            var identifiers = resource.Identifier;
                            identifiers?.ForEach(identifier =>
                            {
                                if (identifier.System!.Contains("Esmd-Idn-UniqueId"))
                                {
                                    identifier.Value = GetSharedGuid();
                                }
                            });

                        }
                    }
                    else if (resource.ResourceType == "List" && resource.Entry != null && resource.Entry.Count > 0)
                    {
                        entry.FullUrl = RequestTransformerUtil.UrnUuidFormattedValue(resourceTypeListGuid);
                        resource.Id = resourceTypeListGuid;
                        var documentReferenceEntry = resource.Entry[0];
                        documentReferenceEntry.Item!.Reference = RequestTransformerUtil.UrnUuidFormattedValue(resourceTypeDocumentReferenceGuid);
                        var extensions = resource.Extension;
                        extensions?.ForEach(extension =>
                        {
                            if (extension.Url!.Contains("Esmd-Ext-UniqueId"))
                            {
                                extension.ValueString = GetSharedGuid();
                            }
                        });

                    }
                }

            }
        }

      

        ~FHIRClientAPIRunnerManager()
        {
            Dispose(false);
        }

    }
}
