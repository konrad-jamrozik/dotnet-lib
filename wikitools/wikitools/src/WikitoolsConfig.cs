using System.IO;
using System.Text.Json;

namespace Wikitools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // - Reason: instantiated by JsonSerializer.Deserialize in Wikitools.WikitoolsConfig.From
    internal record WikitoolsConfig(
        string GitExecutablePath,
        string GitRepoClonePath,
        int GitLogDays,
        string AdoWikiUri,
        string AdoPatEnvVar,
        int AdoWikiPageViewsForDays)
    {
        public static WikitoolsConfig From(string cfgFileName)
        {
            var cfgFilePath = FindConfigFilePath(cfgFileName);
            WikitoolsConfig cfg = null;
            if (File.Exists(cfgFilePath))
                cfg = JsonSerializer.Deserialize<WikitoolsConfig>(File.ReadAllBytes(cfgFilePath),
                    new JsonSerializerOptions{ReadCommentHandling = JsonCommentHandling.Skip});

            return cfg;
        }

        private static string FindConfigFilePath(string cfgFileName)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var cfgFilePath = Path.Combine(dir.FullName, cfgFileName);

            while (!File.Exists(cfgFilePath) && dir.Parent != null)
            {
                dir = dir.Parent;
                cfgFilePath = Path.Combine(dir.FullName, cfgFileName);
            }

            return dir.Parent != null 
                ? cfgFilePath 
                : null;
        }
    }
}