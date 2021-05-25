using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json
{
    public record Configuration(IFileSystem FS)
    {
        /// <summary>
        /// Reads configuration TCfg from the file system FS.
        ///
        /// The contents of TCfg are populated from multiple files.
        /// The file names from which the configuration data is read are deduced
        /// based on naming convention, where the input is the names of the data properties of the type TCfg.
        /// 
        /// Type T data properties are interpreted as two kinds: leaf and composite.
        ///
        ///   Property is a leaf property if its name does not end with IConfiguration.ConfigSuffix.
        ///   Leaf properties will be deserialized from json directly.
        ///   Examples include: string, array of integers.
        ///   These properties will be read from a file named IConfiguration.FileName(typeof(T)).
        ///
        ///   Property is a composite property if its name ends with IConfiguration.ConfigSuffix.
        ///   Composite properties will be interpreted recursively by this algorithm
        ///   before being deserialized from json.
        ///   To read data for such composite property, the algorithm descends into file named 
        ///   IConfiguration.FileName(typeof(TCfg.CompositeProperty)).
        ///
        /// As a result of the algorithm listed above, each project that requires configuration
        /// is expected to copy to output dir its configuration file,
        /// so it can be read into composite property of the parent project.
        /// Moreover, its configuration file name needs to match to the naming convention.
        /// For example, if a parent project is reading TCfg, and it has property named "FooCfg",
        /// expected to contain configuration for project Foo, then the project Foo configuration file name
        /// has to be IConfiguration.FileName(typeof(FooCfg)).
        /// 
        /// </summary>
        public TCfg Read<TCfg>() where TCfg : IConfiguration
        {
            var (cfgFilePath, propCfgs) = ConfigFilePaths<TCfg>();
            JsonElement mergedJson = FS.ReadAllJson(cfgFilePath);
            foreach (var (propName, propCfgFilePath) in propCfgs)
            {
                JsonElement propCfgJsonElement = FS.ReadAllJson(propCfgFilePath);
                mergedJson = mergedJson.Append(propName, propCfgJsonElement);
            }
            return mergedJson.ToObject<TCfg>()!;
        }

        private (string, IDictionary<string, string>) ConfigFilePaths<TCfg>() where TCfg : IConfiguration
        {
            var cfgFileName = IConfiguration.FileName(typeof(TCfg));
            var cfgFilePath = FindConfigFilePath(FS, cfgFileName);
            if (cfgFilePath == null) 
                throw new Exception($"Failed to find {cfgFileName}."); 

            var cfgProps = typeof(TCfg).GetProperties().Where(prop => prop.Name.EndsWith(IConfiguration.ConfigSuffix));
            var propCfgs = cfgProps.ToDictionary(
                cfgProp => cfgProp.Name,
                cfgProp =>
                {
                    var propCfgFileName = IConfiguration.FileName(cfgProp.PropertyType);
                    var propPath = FindConfigFilePath(FS, propCfgFileName);
                    if (propPath == null)
                        throw new Exception($"Failed to find {propCfgFileName}.");
                    return propPath;
                });

            return (cfgFilePath, propCfgs);
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