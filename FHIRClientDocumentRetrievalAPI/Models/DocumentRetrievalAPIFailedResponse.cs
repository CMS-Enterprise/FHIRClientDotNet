using System.Text.Json.Serialization;

namespace FHIRClientDocumentRetrievalAPI.Models
{
 

    public class Details
    {
        [JsonPropertyName("coding")]
        public List<Coding>? Coding { get; set; }
    }

    public class Issue
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("extension")]
        public List<Extension>? Extension { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("severity")]
        public string? Severity { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("details")]
        public Details? Details { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("diagnostics")]
        public string? Diagnostics { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("location")]
        public List<string>? Location { get; set; }
    }


    public class DocumentRetrievalAPIFailedResponse
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Meta? Meta { get; set; }

        [JsonPropertyName("issue")]
        public List<Issue>? Issue { get; set; }
    }


}
