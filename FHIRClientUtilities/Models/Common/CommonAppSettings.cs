namespace FHIRClientUtilities.Models.Common
{
    public class CommonAppSettings
    {
        public string? ContentType { get; set; }
        public string? EndpointURL { get; set; }

        public double HttpClientRequestTimeOutSeconds { get; set; } = 2.0;


    }
}
