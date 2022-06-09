﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Storage;

public record MonthlyJsonFilesStorage(Dir StorageDir)
{
    public T Read<T>(DateTime date)
    {
        // kja need to support here deserializing DateDay, after I changed Wikitools.AzureDevOps.WikiPageStats.DayStat.Day from DateTime to DateDay
        //
        // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-6-0
        var fileToReadName = FileName(date);
        return !StorageDir.FileExists(fileToReadName)
            ? JsonSerializer.Deserialize<T>("[]")!
            : JsonSerializer.Deserialize<T>(StorageDir.ReadAllText(fileToReadName))!;
    }

    public async Task With<T>(DateMonth date, Func<T, T> mergeFunc) where T : class => 
        await Write(mergeFunc(Read<T>(date)), date);

    public Task Write(object data, DateMonth date, string? fileName = default) =>
        WriteToFile(data.ToJsonIndentedUnsafe(), date, fileName);

    private async Task WriteToFile(string dataJson, DateMonth date, string? fileName) =>
        await StorageDir
            .CreateDirIfNotExists()
            .WriteAllTextAsync(fileName ?? FileName(date), dataJson);

    private static string FileName(DateTime date) => $"date_{date:yyyy_MM}.json";
}