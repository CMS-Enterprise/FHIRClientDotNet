using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class UploadClinicalDocumentAPI : CommonAppSettings
    {
        public string? FileName { get; set; }
        public string? ContentMD5 { get; set; }

    }
}
