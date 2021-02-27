using System;
using System.Text.Json; 
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonElementDiff.Value.
using DiffObject = System.Object;

namespace Wikitools.Lib.Json
{
    public class JsonDiff
    {
        private static readonly JsonElement EmptyDiff = JsonSerializer.Deserialize<JsonElement>("{}");

        private readonly Lazy<string> _string;
        private readonly Lazy<string> _rawString;
        private readonly Lazy<JsonElement> _jsonElement;

        public JsonDiff(object baseline, object target)
        {
            var diff = new Lazy<DiffObject>(() =>
            {
                JsonDocument baselineJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(baseline));
                JsonDocument targetJson   = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(target));
                DiffObject?  elementDiff  = new JsonElementDiff(baselineJson, targetJson).Value;
                return elementDiff ?? EmptyDiff;
            });

            _string = new Lazy<string>(() => Json.SerializeIndentedUnsafe(diff.Value));

            _rawString = new Lazy<string>(() => Json.SerializeUnsafe(diff.Value));

            _jsonElement = new Lazy<JsonElement>(() => Json.DeserializeUnsafe<JsonElement>(_rawString.Value));
        }

        public bool IsEmpty => JsonElement.GetRawText() == EmptyDiff.GetRawText();

        public override string ToString() => _string.Value;

        public string ToRawString() => _rawString.Value;

        public JsonElement JsonElement => _jsonElement.Value;
    }
}