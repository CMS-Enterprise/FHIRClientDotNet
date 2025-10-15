using FHIRClientConsoleApp;
using FHIRClientUploadClinicalDocumentAPI.Models;
using NLog;



class Program
{


    static async Task Main(string[] args)
    {

        var runManager = new FHIRClientAPIRunnerManager("nlog.config");

        var token = await runManager.GetTokenAsync();

        if (token.Error == null && token.Value != null)
        {

            var presignedURLDataList = await runManager.GetPresignedURLInfoListAsync(token);


            if (presignedURLDataList != null && presignedURLDataList.Count > 0)
            {
                foreach (var presignedURL in presignedURLDataList)
                {
                    var uploadClinicalDocument = await runManager.UploadClinicalDocumentAsync(presignedUrl: presignedURL.PartValueUrl?.ValueUrl!,
                                                                                          fileName: presignedURL?.PartValueString?.ValueString!, overriddenToken: token);

                    if(uploadClinicalDocument.Value.GetType()== typeof(UploadClinicalDocumentSuccessResponse))
                    {
                        var bundleSubmission = await runManager.SubmitBundleAsync(uploadClinicalDocumentSuccessResponse: (UploadClinicalDocumentSuccessResponse)uploadClinicalDocument.Value,
                                                                                                                        overriddenToken:token);
                    }
                  
                }


            }
            else
            {
                LogManager.GetCurrentClassLogger().Error("No Presigned URL genereated. Check logs for errors.");
            }
            await runManager.GetNotificationsAsync(overriddenToken: token);
            await runManager.RetrieveDocumentAsync(overriddenToken: token);
            await runManager.ProcessDeliveryConfirmationAsync(overriddenToken: token);
            await runManager.ProcessPractitionerRequestAsync(overriddenToken: token);
            await runManager.GetBinaryFileDataAsync(overriddenToken: token);
            await runManager.SubmitBundlePractitionerAsync(overriddenToken: token);

        }



    }

}
