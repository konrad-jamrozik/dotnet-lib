using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wikitools.Lib.Primitives
{
    public class DateDayJsonConverter : JsonConverter<DateDay>
    {
        private const string Format = "yyyy-MM-ddK";

        public override DateDay Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        // kj2 ensure the DateDay has kind UTC. Write a unit test for this.
        {
            string dateToParse = reader.GetString() ?? string.Empty;
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
}