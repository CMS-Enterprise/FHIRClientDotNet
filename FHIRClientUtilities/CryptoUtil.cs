using System.Security.Cryptography;

namespace FHIRClientUtilities
{
    public static class CryptoUtil

    {
       
        public static string ComputeContentMd5String(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            using var stream = File.OpenRead(filePath);
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(stream);

            return Convert.ToBase64String(hashBytes);
        }
        public static byte[] ComputeContentMd5Bytes(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            using var stream = File.OpenRead(filePath);
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(stream);
            return hashBytes;
           
        }

        public static byte[] ConvertBase64StringToBytes(string base64StringValue)
        {
            return Convert.FromBase64String(base64StringValue);
        }

        public static string ComputeSHA256Checksum(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            using FileStream stream = File.OpenRead(filePath);
            using SHA256 sha256 = SHA256.Create();

            byte[] hashBytes = sha256.ComputeHash(stream);

            // Convert to hex string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

    }
}
