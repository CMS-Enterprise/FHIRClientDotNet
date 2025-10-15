using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class DeliveryConfirmationAPI : CommonAppSettings
    {
        public string? Accept { get; set; }
        public DeliveryCofirmationAPIRequest? Request { get; set; }

    }

}
