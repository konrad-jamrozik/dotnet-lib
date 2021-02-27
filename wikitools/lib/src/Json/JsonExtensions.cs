using System.Text.Encodings.Web;
using System.Text.Json;

namespace Wikitools.Lib.Json
{
    public static class JsonExtensions
    {
        private const int MaxDepth = 64;

        private static readonly JsonSerializerOptions SerializerOptionsUnsafe = new()
        {
            MaxDepth = MaxDepth,
            // Necessary to avoid escaping "+" and others characters.
            // Reference:
            // Explanation of how to avoid escaping:
            // https://stackoverflow.com/questions/58003293/dotnet-core-system-text-json-unescape-unicode-string
            // Issue reporting this, with links to discussions explaining the default escaping behavior:
            // https://github.com/dotnet/runtime/issues/29879
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions SerializerOptionsIndentedUnsafe =
            new(SerializerOptionsUnsafe) { WriteIndented = true };

        public static T FromJsonUnsafe<T>(this string json) =>
            JsonSerializer.Deserialize<T>(json, SerializerOptionsUnsafe)!;

        public static string ToJsonUnsafe(this object data) => 
            JsonSerializer.Serialize(data, SerializerOptionsUnsafe);

        public static string ToJsonIndentedUnsafe(this object data) =>
            JsonSerializer.Serialize(data, SerializerOptionsIndentedUnsafe);
    }
}