using System.Text.Json.Serialization;

namespace FHIRClientUploadClinicalDocumentAPI.Models
{
   
    public class Error
    {
        [JsonPropertyName("Code")]
        public required string Code { get; set; }

        [JsonPropertyName("Message")]
        public required string Message { get; set; }

        [JsonPropertyName("X-Amz-Expires")]
        public required string XAmzExpires { get; set; }

        [JsonPropertyName("Expires")]
        public required string Expires { get; set; }

        [JsonPropertyName("ServerTime")]
        public required string ServerTime { get; set; }

        [JsonPropertyName("RequestId")]
        public required string RequestId { get; set; }

        [JsonPropertyName("HostId")]
        public required string HostId { get; set; }
    }

    public class UploadClinicalDocumentFailedResponse
    {
        [JsonPropertyName("Error")]
        public required Error Error { get; set; }
    }


}
