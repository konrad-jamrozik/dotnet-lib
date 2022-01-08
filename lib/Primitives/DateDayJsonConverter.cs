using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wikitools.Lib.Primitives;

// kja delete DateDayJsonConverter once config migrated
public class DateDayJsonConverter : JsonConverter<DateDay>
{
    private const string Format = "yyyy-MM-ddK";

    public override DateDay Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string dateToParse = reader.GetString() ?? string.Empty;
        // kj2 Write unit test showing that the date is roundtripped with Kind.UTC if
        // time zone is Z, and otherwise Unspecified.
        DateTime parsedDate = DateTime.ParseExact(
            dateToParse,
            Format,
            CultureInfo.InvariantCulture, 
            DateTimeStyles.RoundtripKind);
        return new DateDay(parsedDate);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateDay dateDay,
        JsonSerializerOptions options)
        => writer.WriteStringValue(
            dateDay.ToString(
                Format,
                CultureInfo.InvariantCulture));
}