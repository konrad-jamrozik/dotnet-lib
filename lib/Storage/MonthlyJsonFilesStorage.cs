using System;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Storage;

public record MonthlyJsonFilesStorage(Dir StorageDir)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonExtensions.SerializerOptionsIndentedUnsafe)
    {
        Converters = { new DateDayJsonConverter() }
    };

    public T Read<T>(DateTime date)
    {
        var fileToReadName = FileName(date);
        return !StorageDir.FileExists(fileToReadName)
            ? JsonSerializer.Deserialize<T>("[]", SerializerOptions)!
            : JsonSerializer.Deserialize<T>(StorageDir.ReadAllText(fileToReadName), SerializerOptions)!;
    }

    public async Task With<T>(DateMonth date, Func<T, T> mergeFunc) where T : class => 
        await Write(mergeFunc(Read<T>(date)), date);

    public Task Write(object data, DateMonth date, string? fileName = default) =>
        WriteToFile(data.ToJsonIndentedUnsafe(SerializerOptions), date, fileName);

    private async Task WriteToFile(string dataJson, DateMonth date, string? fileName) =>
        await StorageDir
            .CreateDirIfNotExists()
            .WriteAllTextAsync(fileName ?? FileName(date), dataJson);

    // kj2 the input should be DateMonth
    private static string FileName(DateTime date) => $"date_{date:yyyy_MM}.json";
}