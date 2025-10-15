using FHIRClientUtilities.Models.Common;


namespace FHIRClientUtilities.Models
{
    public class PresignedURLAPI: CommonAppSettings
    {
       public string? Accept {  get; set; }
       public PresignedURLAPIRequest? Request {  get; set; }


    }
}
