using FHIRClientUtilities.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime;


namespace FHIRClientUtilities
{

    public static class SettingsUtil
    {
        private static IConfigurationRoot _configuration;
        private static readonly AppSettings _settings;

        static SettingsUtil()
        {

            // Load appsettings.json from current directory
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Bind AppSettings section to AppSettings object
            var initSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

            var jsonSettings = JsonUtil.ToJson(initSettings);

            jsonSettings = jsonSettings.Replace("${FHIRServerUrl}", initSettings.FHIRServerUrl!);
            jsonSettings = jsonSettings.Replace("${EndPointBaseUrl}", initSettings.EndPointBaseUrl!);

            _settings = JsonUtil.FromJson<AppSettings>(jsonSettings) ?? new AppSettings();

            string? clientId = string.IsNullOrEmpty(_settings.AuthenticationAPI!.ClientId) ? Environment.GetEnvironmentVariable("CLIENT_ID") : "";
            string? clientSecret = string.IsNullOrEmpty(_settings.AuthenticationAPI!.ClientSecret) ? Environment.GetEnvironmentVariable("CLIENT_SECRET") : "";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                var msg = "Client ID or Secret is missing in in both appSettings.json and as environment variables. Please set these values either place!";
                Console.WriteLine(msg);
                throw new Exception(msg);


            }
            else
            {
                Console.WriteLine($"Client ID is {clientId} and Secret is {clientSecret} successfully retrieved.");
                _settings.AuthenticationAPI!.ClientId = clientId;
                _settings.AuthenticationAPI!.ClientSecret = clientSecret;
                // Use clientId and clientSecret here
            }

            _settings.BaseFileLocationFolder = FileUtil.EnsureCreateFolderExists(_settings.BaseFileLocationFolder);
            Console.WriteLine($"Base Folder Location {_settings.BaseFileLocationFolder}");


        }

        public static AppSettings GetSettings() => _settings;

    }

}
