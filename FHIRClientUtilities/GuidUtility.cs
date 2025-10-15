namespace FHIRClientUtilities
{
    public static class GuidUtility
    {
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
