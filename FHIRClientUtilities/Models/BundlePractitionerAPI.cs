using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class BundlePractitionerAPI : CommonAppSettings
    {
        public string? Accept { get; set; }
        public BundlePractitionerAPIRequest? Request { get; set; }
    }
}
