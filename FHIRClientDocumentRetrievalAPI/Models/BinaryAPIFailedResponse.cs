using System.Text.Json.Serialization;

namespace FHIRClientDocumentRetrievalAPI.Models
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
   
    public class BinaryAPIFailedResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("issue")]
        public List<Issue>? Issue { get; set; }
    }


}
