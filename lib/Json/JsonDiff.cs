using System;
using System.Text.Json; 
// For explanation of this alias, please see comment on Wikitools.Lib.Json.JsonElementDiff.Value.
using DiffObject = System.Object;

namespace Wikitools.Lib.Json;

// Known limitation:
// Doesn't support tuples
// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0
// Links to GitHub issues:
// - "Tuple support in System.Text.Json.Serializer?"
//   https://github.com/dotnet/runtime/issues/552
// - Possible workaround in:
//   "ValueTuple is not Supported in Aspnet Core 3.1 with default Json."
//   https://github.com/dotnet/runtime/issues/1519#issuecomment-572751931
public class JsonDiff
{
    private static readonly JsonElement EmptyDiff = JsonSerializer.Deserialize<JsonElement>(Json.Empty);

    private readonly Lazy<string> _string;
    private readonly Lazy<string> _rawString;
    private readonly Lazy<JsonElement> _jsonElement;

    public JsonDiff(object baseline, object target)
    {
        var diff = new Lazy<DiffObject>(() =>
        {
            // kj2-json JsonElement deserialization could possibly be simplified to
            // JsonElement baselineJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.SerializeToUtf8Bytes(baseline));
            // kj2-json use instead Wikitools.Lib.Json.JsonExtensions.FromObjectToJsonElement
            JsonDocument baselineJson = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(baseline));
            JsonDocument targetJson   = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(target));
            DiffObject?  elementDiff  = new JsonElementDiff(baselineJson, targetJson).Value;
            return elementDiff ?? EmptyDiff;
        });

        _string = new Lazy<string>(() => diff.Value.ToJsonIndentedUnsafe());

        _rawString = new Lazy<string>(() => diff.Value.ToJsonUnsafe());

        _jsonElement = new Lazy<JsonElement>(() => _rawString.Value.FromJsonToUnsafe<JsonElement>());
    }

    public bool IsEmpty => JsonElement.GetRawText() == EmptyDiff.GetRawText();

    public override string ToString() => _string.Value;

    public string ToRawString() => _rawString.Value;

    public JsonElement JsonElement => _jsonElement.Value;
}