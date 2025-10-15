using System.Text.Json.Serialization;

namespace FHIRClientUtilities.Models
{
    public class RequestParameter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("inject")]
        public bool? Inject { get; set; }
    }
}
