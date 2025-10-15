using System.Text.Json.Serialization;

namespace FHIRClientBundleSubmission.Models
{
    public class BundlePractitionerAPISuccessResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("identifier")]
        public Identifier? Identifier { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("link")]
        public List<Link>? Link { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("entry")]
        public List<Entry>? Entry { get; set; }
    }
  

}
