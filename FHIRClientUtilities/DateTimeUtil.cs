namespace FHIRClientUtilities
{
    public static class DateTimeUtil
    {
        
        public static string GetCurrentUtc()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

      
        public static string FormatAsUtc(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string GetCurrentWithOffset()
        {
            var now = DateTimeOffset.Now;
            return now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }
    }
}
