using System;
using System.Text.Json;
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonDocumentDiff.Value.
using JsonElementDiff = System.Object;

namespace Wikitools.Lib.Json
{
    public class JsonDiff
    {
        private const int MaxDepth = 64;

        private readonly Lazy<JsonElementDiff> _diff;

        public JsonDiff(object baseline, object target)
        {
            _diff = new Lazy<JsonElementDiff>(() =>
            {
                var baselineJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(baseline));
                var targetJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(target));
                JsonElementDiff diff = new JsonDocumentDiff(baselineJson, targetJson).Value;
                return diff;
            });
        }

        public bool IsEmpty => _diff.Value == null;

        public override string ToString()
            => JsonSerializer.Serialize(_diff.Value,
                new JsonSerializerOptions {WriteIndented = true, MaxDepth = MaxDepth});

        public JsonDocument JsonDocument =>
            JsonDocument.Parse(
                JsonSerializer.SerializeToUtf8Bytes(
                    _diff.Value,
                    new JsonSerializerOptions {MaxDepth = MaxDepth}),
                new JsonDocumentOptions {MaxDepth = MaxDepth});
    }
}