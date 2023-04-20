using System.Text.Json.Serialization;
using System.Text.Json;
using NUnit.Framework;

namespace DefaultJsonConverterRepro;

public class MyCustomConverterTests
{
    [Test]
    public static void Test()
    {
        var employee = new Employee();

        // This is going to throw NRE on s_defaultConverter.Write(writer, value, options);
        string serialized = JsonSerializer.Serialize(
            employee,
            new JsonSerializerOptions
            {
                Converters = { new MyCustomConverter() },
            });
        Console.Out.WriteLine("serialized: " + serialized);
    }
}

public class MyCustomConverter : JsonConverter<Employee>
{
    private readonly static JsonConverter<Employee> s_defaultConverter =
        (JsonConverter<Employee>)JsonSerializerOptions.Default.GetConverter(typeof(Employee));

    // Custom serialization logic
    public override void Write(Utf8JsonWriter writer, Employee value, JsonSerializerOptions options)
    {
        s_defaultConverter.Write(writer, value, options);
    }

    // Fall back to default deserialization logic
    public override Employee Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return s_defaultConverter.Read(ref reader, typeToConvert, options)!;
    }
}

public class Employee
{
}