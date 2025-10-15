
using System.Text.Json.Serialization;

namespace FHIRClientAuthenticationAPI.Models
{
    public class Token : TokenFailedResponse
    {
        [JsonPropertyName("expires_in")]
        public double? ExpiresIn {  get; set; }

        [JsonPropertyName("token_type")]
        public string? Type {  get; set; }

        [JsonPropertyName("access_token")]
        public string? Value { get; set; }

        [JsonPropertyName("issued_at")]
        public DateTime? IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
