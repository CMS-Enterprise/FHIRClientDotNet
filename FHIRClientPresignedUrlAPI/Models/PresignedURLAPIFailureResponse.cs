using System.Text.Json.Serialization;

namespace FHIRClientPresignedUrlAPI.Models
{
    
    public class Coding
    {
        [JsonPropertyName("system")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? System { get; set; }

        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Code { get; set; }

        [JsonPropertyName("display")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Display { get; set; }
    }

    public class Details
    {
        [JsonPropertyName("coding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  List<Coding>? Coding { get; set; }
    }

    public class Issue
    {
        [JsonPropertyName("severity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Severity { get; set; }

        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Code { get; set; }

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  Details? Details { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("profile")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  List<string>? Profile { get; set; }
    }

    public class PresignedURLAPIFailureResponse
    {
        [JsonPropertyName("resourceType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? ResourceType { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  Meta? Meta { get; set; }

        [JsonPropertyName("issue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  List<Issue>? Issue { get; set; }
    }


}
