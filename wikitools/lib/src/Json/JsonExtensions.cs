using System.Text.Encodings.Web;
using System.Text.Json;

namespace Wikitools.Lib.Json
{
    public static class JsonExtensions
    {
        private const int MaxDepth = 64;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            MaxDepth = MaxDepth,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static readonly JsonSerializerOptions SerializerOptionsUnsafe = new(SerializerOptions)
        {
            // Necessary to avoid escaping "+" and other characters.
            // Reference:
            // Explanation of how to avoid escaping:
            // https://stackoverflow.com/questions/58003293/dotnet-core-system-text-json-unescape-unicode-string
            // Issue reporting this, with links to discussions explaining the default escaping behavior:
            // https://github.com/dotnet/runtime/issues/29879
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private static readonly JsonSerializerOptions SerializerOptionsUnsafeIgnoreNulls = new(SerializerOptionsUnsafe)
        {
            IgnoreNullValues = true
        };

        private static readonly JsonSerializerOptions SerializerOptionsIndentedUnsafe =
            new(SerializerOptionsUnsafe) { WriteIndented = true };

        public static T FromJsonTo<T>(this string json) =>
            JsonSerializer.Deserialize<T>(json, SerializerOptions)!;

        public static T FromJsonTo<T>(this byte[] bytes) =>
            JsonSerializer.Deserialize<T>(bytes, SerializerOptions)!;

        public static T FromJsonToUnsafe<T>(this string json) =>
            JsonSerializer.Deserialize<T>(json, SerializerOptionsUnsafe)!;

        public static string ToJsonUnsafe(this object data, bool ignoreNulls = false) => 
            JsonSerializer.Serialize(data, ignoreNulls ? SerializerOptionsUnsafeIgnoreNulls : SerializerOptionsUnsafe);

        public static string ToJsonIndentedUnsafe(this object data) =>
            JsonSerializer.Serialize(data, SerializerOptionsIndentedUnsafe);

        /// <remarks>
        /// Based on https://stackoverflow.com/a/58193164/986533
        /// Issue for native support of converting DOM (JsonElement) to a typed object:
        /// https://github.com/dotnet/runtime/issues/31274
        /// </remarks>
        public static T? ToObject<T>(this JsonElement element)
        {
            string json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }

        public static T? ToObject<T>(this JsonDocument document) => ToObject<T>(document.RootElement);

        /// <remarks>
        /// Issue for native support of JSON merging:
        /// https://stackoverflow.com/questions/58694837/system-text-json-merge-two-objects
        /// </remarks>
        public static JsonElement Append(this JsonElement target, string propertyName, JsonElement appended)
        {
            // kja 9 implement based on:
            // Wikitools.Lib.Tests.Json.ConfigurationTests.JsonScratchpad
            // Wikitools.Lib.Tests.Json.ConfigurationTests.JsonScratchpad2
            return target;
        }
    }
}