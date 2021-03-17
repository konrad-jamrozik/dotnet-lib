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
    public record MonthlyJsonFilesStorage(IFileSystem FileSystem, string StorageDirPath)
    {
        public T Read<T>(DateTime date)
        {
            var fileToReadName = $"date_{date:yyy_MM}.json";
            var fileToReadPath = FileSystem.JoinPath(StorageDirPath, fileToReadName);
            return !FileSystem.FileExists(fileToReadPath)
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
            WriteToFile(data.ToJsonIndentedUnsafe(), date, fileNameOverride);

        private async Task WriteToFile(string dataJson, DateTime date, string? fileNameOverride)
        {
            fileNameOverride ??= $"date_{date:yyy_MM}.json";
            var storageDir = new Dir(FileSystem, StorageDirPath);
            if (!storageDir.Exists())
                FileSystem.CreateDirectory(storageDir.Path);
            var filePath = FileSystem.JoinPath(StorageDirPath, fileNameOverride);
            await FileSystem.WriteAllTextAsync(filePath, dataJson);
        }
    }
}