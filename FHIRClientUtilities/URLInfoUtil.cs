using System.Web;

namespace FHIRClientUtilities
{
    public class UrlInfo
    {
        public string BaseUrl { get; set; } = string.Empty;
        public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
    }

    public static class URLInfoUtil
    {
        public static UrlInfo ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            var uri = new Uri(url);

            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var query = uri.Query; // includes leading '?'

            if (!string.IsNullOrEmpty(query))
            {
                // Remove the leading '?'
                var queryString = query.Substring(1);

                var parsedQuery = HttpUtility.ParseQueryString(queryString);

                foreach (string key in parsedQuery)
                {
                    if (key == null) continue; // ignore null keys
                    if (parsedQuery[key] != null)
                    {
                        queryParams[key] = parsedQuery[key]!;
                    }
                    

                }
            }

            return new UrlInfo
            {
                BaseUrl = uri.GetLeftPart(UriPartial.Path),
                QueryParams = queryParams
            };
        }
    }
}
