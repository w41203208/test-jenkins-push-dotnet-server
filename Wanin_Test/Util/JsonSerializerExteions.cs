using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Wanin_Test.Util
{
    public static class JsonSerializerExteions
    {
        public static string SerializeWithCamelCase<T>(T data, JsonSerializerOptions? options = null)
        {
            var resultOptions = GetSerializerOptions(options);
            return JsonSerializer.Serialize(data, resultOptions);
        }
        public static T? DeserializeFromCamelCase<T>(string json, JsonSerializerOptions? options = null)
        {
            var resultOptions = GetSerializerOptions(options);
            return JsonSerializer.Deserialize<T>(json, resultOptions);
        }

        private static JsonSerializerOptions GetSerializerOptions(JsonSerializerOptions? options = null)
        {
            var defaultOptions = new JsonSerializerOptions
            {
                // 這個是用來在轉換時，從原來的字串 ex: HelloWorld -> helloWorld
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            JsonSerializerOptions resultOptions;
            if (options == null)
            {
                resultOptions = defaultOptions;
            }
            else
            {
                resultOptions = options;
            }
            return resultOptions;
        }
    }
}
