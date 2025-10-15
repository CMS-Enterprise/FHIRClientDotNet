using System.Text.Json.Serialization;

namespace FHIRClientUtilities.Models
{
   
    public class Address
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("use")]
        public string? Use { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("line")]
        public List<string>? Line { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }
    }




    public class Name
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("family")]
        public string? Family { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("given")]
        public List<string>? Given { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("prefix")]
        public List<string>? Prefix { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("suffix")]
        public List<string>? Suffix { get; set; }
    }

    public class PractitionerAPIRequest
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
        [JsonPropertyName("name")]
        public List<Name>? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("telecom")]
        public List<Telecom>? Telecom { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("address")]
        public List<Address>? Address { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("extension")]
        public List<Extension>? Extension { get; set; }
    }


    public class Telecom
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        [JsonPropertyName("use")]
        public string? Use { get; set; }
    }


}
