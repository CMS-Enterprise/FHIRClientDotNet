namespace FHIRClientUtilities
{
    public static class TokenUtil
    {
        public static bool IsTokenExpiredMS(long tokenExpiryMs)
        {
            // Get current UTC time in Unix milliseconds
            long currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Compare
            return currentTimeMs >= tokenExpiryMs;
        }
        public static bool IsTokenExpired(DateTime issuedAt, double expiresInSeconds)
        {
            DateTime expirationTime = issuedAt.AddSeconds(expiresInSeconds);
            return DateTime.UtcNow >= expirationTime;
        }
    }
}
