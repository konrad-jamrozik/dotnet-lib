using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wikitools.Lib.Primitives;

public class DateDayJsonConverter : JsonConverter<DateDay>
{
    // Custom format string documentation:
    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
    private const string SerializationFormat = "yyyy-MM-ddK";

    // Required to parse the DeserializableFormats, per:
    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier
    private const DateTimeStyles SerializationDateTimeStyles = DateTimeStyles.RoundtripKind;

    // Per empirical observation, this is the default JsonSerializer serialization format.
    //
    // However, in actuality, it should be round-trip ISO 8061:
    // https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
    // which would be represented by "o", per:
    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-round-trip-o-o-format-specifier
    // This custom format is basically "o" but with fractions of seconds omitted.
    //
    // Custom format string documentation: 
    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
    private const string DefaultSerializationFormat = "yyyy-MM-ddTHH:mm:ssK";

    // kj2-tests Write unit test showing both of these format can be deserialized successfully.
    // Need two deserialization formats because SerializationFormat is the new format I
    // started to use for serialization. However, before I started doing it, some of the data was already
    // serialized with DefaultSerializationFormat.
    private static readonly string[] DeserializationFormats = { SerializationFormat, DefaultSerializationFormat };

    private static readonly CultureInfo SerializationCulture = CultureInfo.InvariantCulture;

    public override DateDay Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string dateToParse = reader.GetString() ?? string.Empty;
        // kj2-tests Write unit test showing that the date is round-tripped with Kind.UTC if
        // time zone is Z, and otherwise Unspecified.
        DateTime parsedDate = DateTime.ParseExact(
            dateToParse,
            DeserializationFormats,
            SerializationCulture, 
            SerializationDateTimeStyles);
        return new DateDay(parsedDate);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateDay dateDay,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            dateDay.ToString(
                SerializationFormat,
                SerializationCulture));
    }
} 