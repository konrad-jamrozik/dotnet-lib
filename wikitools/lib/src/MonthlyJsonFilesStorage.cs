using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.OS;

namespace Wikitools.Lib
{
    public record MonthlyJsonFilesStorage(IFileSystem FileSystem, string StorageDirPath)
    {
        public T Read<T>(DateTime date)
        {
            var fileToReadName = $"date_{date:yyy_MM}.json";
            // kja FileSysytem for Path and Exists
            var fileToReadPath = Path.Join(StorageDirPath, fileToReadName);
            return !File.Exists(fileToReadPath)
                ? JsonSerializer.Deserialize<T>("[]")!
                : JsonSerializer.Deserialize<T>(FileSystem.ReadAllText(fileToReadPath))!;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        // reason: Fluent interface
        public async Task<MonthlyJsonFilesStorage> With<T>(DateTime date, Func<T, T> mergeFunc) where T : class
        {
            T storedData = Read<T>(date);
            T mergedData = mergeFunc(storedData);
            await Write(mergedData, date);
            return this;
        }

        public Task Write(object data, DateTime date, string? fileNameOverride = default) =>
            WriteToFile(ToJson(data), date, fileNameOverride);

        private async Task WriteToFile(string dataJson, DateTime date, string? fileNameOverride)
        {
            fileNameOverride ??= $"date_{date:yyy_MM}.json";
            var storageDir = new Dir(FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                FileSystem.CreateDirectory(storageDir.Path);
            var filePath = FileSystem.JoinPath(StorageDirPath, fileNameOverride);
            await FileSystem.WriteAllTextAsync(filePath, dataJson);
        }

        private string ToJson(object data) =>
            // kja dedup with JsonDiff
            JsonSerializer.Serialize(data,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });
    }
}