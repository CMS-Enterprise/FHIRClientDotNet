
using System.Text.Json.Serialization;


namespace FHIRClientPresignedUrlAPI.Models
{


    public class PresignedURLAPIResponse
    {
        [JsonPropertyName("resourceType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ResourceType { get; set; }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        [JsonPropertyName("parameter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Parameter>? Parameter { get; set; }
    }



    public class Parameter
    {
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonPropertyName("part")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  List<Part>? Part { get; set; }

        [JsonPropertyName("valueDuration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  ValueDuration? ValueDuration { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Name { get; set; }

        [JsonPropertyName("part")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  List<Part>? PartItem { get; set; }

        [JsonPropertyName("valueString")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValueString { get; set; }

        [JsonPropertyName("valueUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValueUrl { get; set; }
    }



    public class ValueDuration
    {
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  int? Value { get; set; }

        [JsonPropertyName("unit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  string? Unit { get; set; }

        [JsonPropertyName("system")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? System { get; set; }

        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }
    }




}
