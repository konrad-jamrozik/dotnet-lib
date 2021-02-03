using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public record MonthlyJsonFilesStore(IOperatingSystem OS, string StorageDirPath)
    {
        public T Read<T>(DateTime date)
        {
            var fileToReadName = $"date_{date:yyy_MM}.json";
            var fileToReadPath = Path.Join(StorageDirPath, fileToReadName);
            var readJsonStr    = File.ReadAllText(fileToReadPath);
            return JsonSerializer.Deserialize<T>(readJsonStr)!;
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
    }
}