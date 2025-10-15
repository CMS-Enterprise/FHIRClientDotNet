using FHIRClientUtilities.Models.Common;
using System.Text.Json.Serialization;

namespace FHIRClientUtilities.Models
{
 
    public class NotificationRetrievalAPI : CommonAppSettings
    {
     

        [JsonPropertyName("Accept")]
        public string? Accept { get; set; }


        [JsonPropertyName("RequestParameters")]
        public List<RequestParameter>? RequestParameters { get; set; }
    }


}
