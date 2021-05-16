using System;
using System.Linq;
using System.Reflection;
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
            // - Read the top-level config .json.

            var cfgFileName = IConfiguration.FileName(typeof(TCfg));
            var cfgFilePath = FindConfigFilePath(FS, cfgFileName);
            Console.Out.WriteLine("path " + cfgFilePath);

            var cfgProps = typeof(TCfg).GetProperties().Where(prop => prop.Name.EndsWith(IConfiguration.ConfigSuffix));
            foreach (PropertyInfo cfgProp in cfgProps)
            {
                var propCfgFileName = IConfiguration.FileName(cfgProp.PropertyType);
                var propCfgFilePath = FindConfigFilePath(FS, propCfgFileName);
                Console.Out.WriteLine("path " + propCfgFilePath);
            }


            // - Read all the keys in it.
            // - Figure out which keys are leafs and which are configs to be composed (ending with Cfg)
            // - For each leaf, read the key-value pair into dynamic object.
            // - For each Cfg, recursively apply the same logic, with appropriately changed top-level config.json
            // - Once the recursion returns the dynamic object, attach it under the Cfg node.
            // - Finally, at the end, convert the entire dynamic object to T.
            throw new NotImplementedException();
        }

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