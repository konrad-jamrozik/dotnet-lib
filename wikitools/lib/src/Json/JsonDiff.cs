using System;
using System.Text.Json;
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonElementDiff.Value.
using DiffObject = System.Object;

namespace Wikitools.Lib.Json
{
    public class JsonDiff
    {
        private const int MaxDepth = 64;

        private readonly Lazy<DiffObject> _diff;
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            MaxDepth = MaxDepth, 
            // Necessary to avoid escaping "+" and others characters.
            // Reference:
            // Explanation of how to avoid escaping:
            // https://stackoverflow.com/questions/58003293/dotnet-core-system-text-json-unescape-unicode-string
            // Issue reporting this, with links to discussions explaining the default escaping behavior:
            // https://github.com/dotnet/runtime/issues/29879
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public JsonDiff(object baseline, object target)
        {
            _diff = new Lazy<DiffObject>(() =>
            {
                var baselineJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(baseline));
                var targetJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(target));
                DiffObject diff = new JsonElementDiff(baselineJson, targetJson).Value;
                return diff;
            });
        }

        public bool IsEmpty => _diff.Value == null;

        public override string ToString()
            => JsonSerializer.Serialize(_diff.Value,
                new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true });

        public string ToRawString()
            => JsonSerializer.Serialize(_diff.Value, JsonSerializerOptions);

        public JsonElement JsonElement => 
            JsonSerializer.Deserialize<JsonElement>(
                JsonSerializer.SerializeToUtf8Bytes(
                    _diff.Value, 
                    JsonSerializerOptions), 
                JsonSerializerOptions);
    }
}