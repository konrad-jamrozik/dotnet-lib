using System;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Configuration
{
    public record Configuration(IFileSystem FS)
    {
        public T Read<T>(string cfgFileName) where T : IConfiguration
        {
            var cfgFilePath = FindConfigFilePath(FS, cfgFileName);
            return cfgFilePath != null && FS.FileExists(cfgFilePath)
                ? FS.ReadAllBytes(cfgFilePath).FromJsonTo<T>()
                : throw new Exception($"Failed to find {cfgFileName}.");
        }

        private static string? FindConfigFilePath(IFileSystem fs, string cfgFileName)
        {
            var dir = fs.CurrentDir;

            var cfgFilePath = dir.JoinPath(cfgFileName);

            while (!fs.FileExists(cfgFilePath) && dir.Parent != null)
            {
                dir = dir.Parent;
                cfgFilePath = dir.JoinPath(cfgFileName);
            }

            return dir.Parent != null ? cfgFilePath : null;
        }
    }
}