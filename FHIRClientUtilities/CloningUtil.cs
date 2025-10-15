
using System.Text.Json;

namespace FHIRClientUtilities
{
    public static class CloningUtil
    {
        public static T DeepClone<T>(T source)
        {
            if (source == null || source.GetType() != typeof(T)) return source;

            var json = JsonSerializer.Serialize(source);
            var result = JsonSerializer.Deserialize<T>(json);
            if (result == null)
            {
                return source;
            }

            return result;
        }
    }
}
