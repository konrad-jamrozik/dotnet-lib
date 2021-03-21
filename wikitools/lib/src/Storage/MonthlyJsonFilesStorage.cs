using System;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Storage
{
    // kj2 instead of passing StorageDirPath separately of FileSystem,
    // introduce IStorageDir class, that will encapsulate both.
    // This way I will need to simulate only the new IStorageDir, not IFileSystem
    public record MonthlyJsonFilesStorage(IFileSystem FileSystem, string StorageDirPath, Dir StorageDir)
    {
        public T Read<T>(DateTime date)
        {
            var fileToReadName = $"date_{date:yyy_MM}.json";
            // kj2 this could be made into an abstraction, like: 
            // read json array from file or return empty array otherwise
            return !StorageDir.FileExists(fileToReadName)
                ? JsonSerializer.Deserialize<T>("[]")!
                : JsonSerializer.Deserialize<T>(StorageDir.ReadAllText(fileToReadName))!;
        }

        public async Task With<T>(DateTime date, Func<T, T> mergeFunc) where T : class => 
            await Write(mergeFunc(Read<T>(date)), date);

        public Task Write(object data, DateTime date, string? fileName = default) =>
            WriteToFile(data.ToJsonIndentedUnsafe(), date, fileName);

        private async Task WriteToFile(string dataJson, DateTime date, string? fileName) =>
            await StorageDir
                .CreateDirIfNotExists()
                .WriteAllTextAsync(fileName ?? $"date_{date:yyy_MM}.json", dataJson);
    }
}