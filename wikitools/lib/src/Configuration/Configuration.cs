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
            // kja 7 config read curr work before reading the file, compose it. I.e. if the T
            // has a key of AzureDevOpsTestsCfg, first find azuredevops-tests_config.json, hydrate it into that type,
            // then compose it into output.
            // This entire logic will likely need to change. To something like:
            // - Read the top-level config .json.
            // - Read all the keys in it.
            // - Figure out which keys are leafs and which are configs to be composed (ending with Cfg)
            // - For each leaf, read the key-value pair into dynamic object.
            // - For each Cfg, recursively apply the same logic, with appropriately changed top-level config. json
            // - Once the recursion returns the dynamic object, attach it under the Cfg node.
            // - Finally, at the end, convert the entire dynamic object to T.
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