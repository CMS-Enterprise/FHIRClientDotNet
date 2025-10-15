using FHIRClientUtilities.Models.Common;

namespace FHIRClientUtilities.Models
{
    public class AuthenticationAPI: CommonAppSettings
    {
      
        public string? ClientId {  get; set; }
        public string? ClientSecret { get; set; }
        public string? Scope { get; set; }
       

    }
}
