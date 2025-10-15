namespace FHIRClientUtilities.Models
{
    public class AppSettings
    {
        public AuthenticationAPI? AuthenticationAPI { get; set; }
        public PresignedURLAPI? PresignedURLAPI { get; set; }
        public UploadClinicalDocumentAPI? UploadClinicalDocumentAPI { get; set; }
        public string? BaseFileLocationFolder { get; set; }
        public BundleSubmissionAPI? BundleSubmissionAPI { get; set; }
        public NotificationRetrievalAPI? NotificationRetrievalAPI { get; set; }
        public DocumentRetrievalAPI? DocumentRetrievalAPI { get; set; }
        public DeliveryConfirmationAPI? DeliveryConfirmationAPI { get; set; }

        public BundlePractitionerAPI? BundlePractitionerAPI { get; set; }

        public BinaryAPI? BinaryAPI { get; set; }

        public PractitionerAPI? PractitionerAPI { get; set; }
        public double HttpClientRequestTimeOutSeconds { get; set; }
        public string? FHIRServerUrl { get; set; }
        public string? EndPointBaseUrl { get; set; }
    }

}

