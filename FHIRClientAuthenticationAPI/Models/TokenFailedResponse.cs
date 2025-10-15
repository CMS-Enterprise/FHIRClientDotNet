using System.Text.Json.Serialization;

namespace FHIRClientAuthenticationAPI.Models
{
    public class TokenFailedResponse
    {
        [JsonPropertyName("error")]
        public string? Error {  get; set; }

        [JsonPropertyName("status_code_value")]
        public int? StatusCodeValue { get; set; }

        [JsonPropertyName("status_code_description")]
        public string? StatusCodeDescription { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

    }
}
