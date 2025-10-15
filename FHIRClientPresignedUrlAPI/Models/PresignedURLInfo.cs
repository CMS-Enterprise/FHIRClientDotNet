namespace FHIRClientPresignedUrlAPI.Models
{
    public  class PresignedURLInfo
    {
       public PartValueString? PartValueString { get; set; }
       public PartValueUrl? PartValueUrl { get; set; }

    }

    public class PartValueString
    {
        public  required string Name { get; set; }
        public  required string ValueString { get; set; }
    }

    public class PartValueUrl
    {
        public required string Name { get; set; }
        public required string ValueUrl { get; set; }
    }
}
