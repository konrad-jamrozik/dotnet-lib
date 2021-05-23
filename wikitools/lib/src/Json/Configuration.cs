using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Json
{
    public record Configuration(IFileSystem FS)
    {
        /// <summary>
        /// Reads configuration TCfg from the file system FS.
        ///
        /// The contents of TCfg are populated from multiple files.
        /// The file names from which the configuration data is read are deduced
        /// based on naming convention, where the input are the names of the data properties of the type TCfg.
        /// 
        /// Type T data properties are interpreted as two kinds: leaf and composite.
        ///
        ///   Property is a leaf property if its name does not end with IConfiguration.ConfigSuffix.
        ///   Leaf properties will be deserialized from json directly.
        ///   Examples include: string, array of integers.
        ///   These properties will be read from a file named IConfiguration.FileName(nameof(T)).
        ///
        ///   Property is a composite property if its name ends with IConfiguration.ConfigSuffix.
        ///   Composite properties will be interpreted recursively by this algorithm
        ///   before being deserialized from json.
        ///   To read data for such composite property, the algorithm descends into file named 
        ///   IConfiguration.FileName(nameof(TCfg.CompositeProperty)).
        ///
        /// As a result of the algorithm listed above, each project that requires configuration
        /// is expected to copy to output dir its configuration file,
        /// so it can be read into composite property of the parent project.
        /// Moreover, its configuration file name needs to match to the naming convention.
        /// For example, if a parent project is reading TCfg, and it has property named "FooCfg",
        /// expected to contain configuration for project Foo, then the project Foo configuration file name
        /// has to be IConfiguration.FileName(nameof(TCfg.FooCfg)).
        /// 
        /// </summary>
        public TCfg Read<TCfg>() where TCfg : IConfiguration
        {
            // kja 7 implement the algorithm described in Read doc.
            // It will likely look something like:
            // - Inspect the TCfg type to find all the *Cfg properties.
            // - Use the reed props to deduce the paths (POCO already done, see ConfigFilePaths)
            // - Read each config into JsonElement
            // - Merge all JsonElements into one; for POCO see Wikitools.Lib.Tests.Json.ConfigurationTests.JsonScratchpad
            // - Load the merged Json into TCfg.
            var configFilePaths = ConfigFilePaths<TCfg>();
            foreach (var configFilePath in configFilePaths)
            {
                Console.Out.WriteLine("path: " + configFilePath);
                // kja 9 besides the paths, I also need reference to all the Cfg types read from TCfg properties, to know how to read the jeson with ReadAllJsonTo.
                // Currently it assumes there are no nested configs, just TCfg.
                // In other words:
                // - make this test pass: Wikitools.Lib.Tests.Json.ConfigurationTests.ComposesAndReadsCompositeConfig
                //   - Part of it is ensuring they have the right test fixtures in the simulated file system
                //   - Review the POCO test in ConfigurationTests showing how to merge JSON ()
                TCfg cfgJson = FS.ReadAllJsonTo<TCfg>(configFilePath);
                return cfgJson;
            }

            throw new InvalidOperationException();
        }

        private List<string> ConfigFilePaths<TCfg>() where TCfg : IConfiguration
        {
            var cfgFileName = IConfiguration.FileName(typeof(TCfg));
            var cfgFilePath = FindConfigFilePath(FS, cfgFileName);

            var cfgProps = typeof(TCfg).GetProperties().Where(prop => prop.Name.EndsWith(IConfiguration.ConfigSuffix));
            var propCfgFilePaths = cfgProps.Select(cfgProp =>
            {
                var propCfgFileName = IConfiguration.FileName(cfgProp.PropertyType);
                return FindConfigFilePath(FS, propCfgFileName);
            });

            return cfgFilePath.AsList().Concat(propCfgFilePaths).Where(path => path != null).Cast<string>().ToList();
        }

        public T Read<T>(string cfgFileName) where T : IConfiguration
        {
            var cfgFilePath = FindConfigFilePath(FS, cfgFileName);

            return cfgFilePath != null && FS.FileExists(cfgFilePath)
                ? FS.ReadAllJsonTo<T>(cfgFilePath)
                : throw new Exception($"Failed to find {cfgFileName}.");
        }

        private static string? FindConfigFilePath(IFileSystem fs, string cfgFileName)
        {
            var dir = fs.CurrentDir;

            var cfgFilePath = dir.JoinPath(cfgFileName);

            do
            {
                if (fs.FileExists(cfgFilePath))
                    return cfgFilePath;

                if (dir.Parent != null)
                {
                    dir = dir.Parent;
                    cfgFilePath = dir.JoinPath(cfgFileName);
                }
                else
                    break;

            } while (true);

            return dir.Parent != null ? cfgFilePath : null;
        }
    }
}