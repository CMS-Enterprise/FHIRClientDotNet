using System.Text.Json.Serialization;

namespace FHIRClientDocumentRetrievalAPI.Models
{

    public class DocumentRetrievalAPISuccessResponse
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("link")]
        public List<Link>? Link { get; set; }

        [JsonPropertyName("entry")]
        public List<Entry>? Entry { get; set; }
    }

    public class Attachment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("creation")]
        public string? Creation { get; set; }
    }

    public class Coding
    {
        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("display")]
        public string? Display { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("attachment")]
        public Attachment? Attachment { get; set; }

        [JsonPropertyName("format")]
        public Format? Format { get; set; }
    }

    public class Context
    {
        [JsonPropertyName("facilityType")]
        public FacilityType? FacilityType { get; set; }
    }

    public class Entry
    {
        [JsonPropertyName("fullUrl")]
        public string? FullUrl { get; set; }

        [JsonPropertyName("resource")]
        public Resource? Resource { get; set; }
    }

    public class Extension
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("valueString")]
        public string? ValueString { get; set; }

        [JsonPropertyName("valueCode")]
        public string? ValueCode { get; set; }
    }

    public class FacilityType
    {
        [JsonPropertyName("coding")]
        public List<Coding>? Coding { get; set; }
    }

    public class Format
    {
        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("display")]
        public string? Display { get; set; }
    }

    public class Identifier
    {
        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class Link
    {
        [JsonPropertyName("relation")]
        public string? Relation { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("profile")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Profile { get; set; }

        [JsonPropertyName("security")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Security>? Security { get; set; }
    }

    public class Resource
    {
        [JsonPropertyName("resourceType")]
        public string? ResourceType { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("meta")]
        public Meta? Meta { get; set; }

        [JsonPropertyName("extension")]
        public List<Extension>? Extension { get; set; }

        [JsonPropertyName("identifier")]
        public List<Identifier>? Identifier { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("securityLabel")]
        public List<SecurityLabel>? SecurityLabel { get; set; }

        [JsonPropertyName("content")]
        public List<Content>? Content { get; set; }

        [JsonPropertyName("context")]
        public Context? Context { get; set; }
    }

    public class Security
    {
        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("display")]
        public string? Display { get; set; }
    }

    public class SecurityLabel
    {
        [JsonPropertyName("coding")]
        public List<Coding>? Coding { get; set; }
    }


}
