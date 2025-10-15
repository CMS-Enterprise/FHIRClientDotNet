using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class BundleSubmissionAPI : CommonAppSettings
    {
        public string? Accept { get; set; }
        public SubmissionBundleAPIRequest? Request { get; set; }
    }
}
