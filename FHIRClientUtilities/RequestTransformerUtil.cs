namespace FHIRClientUtilities
{
    public static class RequestTransformerUtil
    {

        public static string UrnUuidFormattedValue(string guid)
        {
            return $"urn:uuid:{guid}";
        }
        

    }
}
