using System.Text.Json.Serialization;

namespace FHIRClientDocumentRetrievalAPI.Models
{

    public class DeliveryConfirmationAPIResponse
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("contained")]
        public List<Contained>? Contained { get; set; }

        [JsonPropertyName("extension")]
        public List<Extension>? Extension { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("entry")]
        public List<Entry>? Entry { get; set; }
    }

    public class Contained
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("issue")]
        public List<Issue>? Issue { get; set; }
    }

  
    public class Item
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }



}
