using System.Text.Json;

namespace FHIRClientUtilities
{
    public static class FileUtil
    {
        public static string GetFullFilePath(string folder, string fileName)
        {
            return Path.Combine(folder, fileName);
        }

        public static double GetFileSizeInMB(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            FileInfo fileInfo = new FileInfo(filePath);
            long sizeInBytes = fileInfo.Length;
            double sizeInMB = sizeInBytes / (1024.0 * 1024.0);

            return sizeInMB;
        }

        public static long GetFileSizeBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        public static void SaveAsJsonFile(object obj, string filePath, bool prettyPrint = true)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided.", nameof(filePath));

            // Ensure .json extension
            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                filePath += ".json";

            var options = new JsonSerializerOptions
            {
                WriteIndented = prettyPrint,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(obj, options);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
        }

        public static string GenerateGuidFilePath(string basePath, string subFolder, string filePrefix, string guid, string extension = ".json")
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentException("Base path is required.", nameof(basePath));

            if (string.IsNullOrWhiteSpace(subFolder))
                throw new ArgumentException("Subfolder name is required.", nameof(subFolder));

            if (string.IsNullOrWhiteSpace(filePrefix))
                throw new ArgumentException("File prefix name is required.", nameof(filePrefix));

            if (string.IsNullOrWhiteSpace(extension))
                extension = ".json";

            if (!extension.StartsWith("."))
                extension = "." + extension;

            string fullDirectoryPath = Path.Combine(basePath, subFolder);

            // Ensure the directory exists
            Directory.CreateDirectory(fullDirectoryPath);

            // Generate GUID filename
            string fileName = filePrefix + "_" + guid + extension;

            return Path.Combine(fullDirectoryPath, fileName);
        }

        public static string EnsureCreateFolderExists(string? folderName, string defaultFolderName = "DefaultFhirClientFolder")
        {

            // Use default folder name if input is invalid
           

            if (string.IsNullOrWhiteSpace(folderName))
            {
                folderName = defaultFolderName;
            }

            string folderPath = string.IsNullOrWhiteSpace(folderName) ? 
                Path.Combine(Directory.GetCurrentDirectory(), folderName) : folderName;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

    }
}
