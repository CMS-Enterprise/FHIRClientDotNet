using System.Text.Json.Serialization;

namespace FHIRClientUploadClinicalDocumentAPI.Models
{
    public class UploadClinicalDocumentSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("filename")]
        public required string Filename { get; set; }

        [JsonPropertyName("s3uri")]
        public required string S3uri { get; set; }
    }

}
