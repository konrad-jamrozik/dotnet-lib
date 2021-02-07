using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record MonthlyJsonFilesStorage(IOperatingSystem OS, string StorageDirPath)
    {
        public T Read<T>(DateTime date)
        {
            var fileToReadName = $"date_{date:yyy_MM}.json";
            var fileToReadPath = Path.Join(StorageDirPath, fileToReadName);
            return !File.Exists(fileToReadPath) 
                ? JsonSerializer.Deserialize<T>("[]")! 
                : JsonSerializer.Deserialize<T>(File.ReadAllText(fileToReadPath))!;
        }

        public DateTime FindMaxDate()
        {
            var      dir     = new Dir(OS.FileSystem, StorageDirPath);
            var      files   = Directory.EnumerateFiles(dir.Path);
            DateTime maxDate = DateTime.MinValue;
            foreach (var file in files)
            {
                Console.Out.WriteLine("file: " + file);
                var dateMatch  = Regex.Match(file, "date_(.*)\\.json");
                var dateString = dateMatch.Groups[1].Value;
                var date       = DateTime.ParseExact(dateString, "yyyy_MM", CultureInfo.InvariantCulture);
                if (date > maxDate)
                {
                    maxDate = date;
                }
            }

            return maxDate;
        }

        public Task Write(object data, DateTime date, string? fileNameOverride = default) =>
            WriteToFile(ToJson(data), date, fileNameOverride);

        private async Task WriteToFile(string dataJson, DateTime date, string? fileNameOverride)
        {
            fileNameOverride ??= $"date_{date:yyy_MM}.json";
            var storageDir = new Dir(OS.FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                Directory.CreateDirectory(storageDir.Path);
            var filePath = Path.Join(StorageDirPath, fileNameOverride);
            await File.WriteAllTextAsync(filePath, dataJson);
        }

        private string ToJson(object data) =>
            JsonSerializer.Serialize(data,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });
    }
}