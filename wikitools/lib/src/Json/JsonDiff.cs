using System;
using System.Text.Encodings.Web;
using System.Text.Json; 
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonElementDiff.Value.
using DiffObject = System.Object;

namespace Wikitools.Lib.Json
{
    public class JsonDiff
    {
        private const int MaxDepth = 64;
        private static readonly JsonElement EmptyDiff = JsonSerializer.Deserialize<JsonElement>("{}");

        private readonly Lazy<string> _string;
        private readonly Lazy<string> _rawString;
        private readonly Lazy<JsonElement> _jsonElement;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
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

        public JsonDiff(object baseline, object target)
        {
            var diff = new Lazy<DiffObject>(() =>
            {
                JsonDocument baselineJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(baseline));
                JsonDocument targetJson   = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(target));
                DiffObject?  elementDiff  = new JsonElementDiff(baselineJson, targetJson).Value;
                return elementDiff ?? EmptyDiff;
            });

            _string = new Lazy<string>(() => JsonSerializer.Serialize(diff.Value,
                new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true }));

            _rawString = new Lazy<string>(() => JsonSerializer.Serialize(diff.Value, JsonSerializerOptions));

            _jsonElement = new Lazy<JsonElement>(() =>
                JsonSerializer.Deserialize<JsonElement>(_rawString.Value, JsonSerializerOptions));
        }

        public bool IsEmpty => JsonElement.GetRawText() == EmptyDiff.GetRawText();

        public override string ToString() => _string.Value;

        public string ToRawString() => _rawString.Value;

        public JsonElement JsonElement => _jsonElement.Value;
    }
}