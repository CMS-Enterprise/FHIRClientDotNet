using System.Text.Json.Serialization;

namespace FHIRClientDocumentRetrievalAPI.Models
{
    public class DeliveryConfirmationAPIFailedResponse
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("issue")]
        public List<Issue>? Issue { get; set; }

    }


}
