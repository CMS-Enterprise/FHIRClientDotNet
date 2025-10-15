using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class PractitionerAPI : CommonAppSettings
    {
        public string? Accept { get; set; }
        public PractitionerAPIRequest? Request { get; set; }
    }
   
}
