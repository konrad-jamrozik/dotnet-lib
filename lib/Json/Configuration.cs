using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

// kj2 need to figure out better way of handling configs, as they are not versioned.
// The config values are not strictly speaking secrets, but I cannot share them as this is in public
// repo yet the config values refer to company-internal artifacts.
// kj2 problem with config refresh. They refresh in the /bin of owning project, but not in the runtime project, so rebuild doesn't refresh them properly.
public record Configuration(IFileSystem FS)
{
    /// <summary>
    /// Reads configuration TCfg from the file system FS.
    ///
    /// ----------------------------------------
    /// Algorithm
    /// 
    /// The TCfg is deserialized from a series of .json configuration files.
    ///
    /// The root config file is returned by invocation of:
    /// 
    /// var rootCfgFilePath
    ///   = FindConfigFilePath(FS, cfgFileName: IConfiguration.FileName(typeof(TCfg));
    ///
    /// All TCfg properties are read directly from rootCfgFilePath into TCfg,
    /// except TCfg properties that end with Cfg.
    /// 
    /// For them, a "property config" file is found and read. The algorithm used for the
    /// "property config" file doesn't allow further nesting of configs.
    ///
    /// ----------------------------------------
    /// Rationale
    /// 
    /// The algorithm listed above is designed to facilitate config encapsulation per project.
    /// That is, if given project Root configuration also leverages configuration
    /// of projects Dep1 and Dep2, then RootCfg is expected to have Dep1Cfg and Dep2Cfg
    /// properties, and the build step is expected to copy relevant config files from the Dep1
    /// and Dep2 projects into dir that will make it discoverable.
    ///
    /// ----------------------------------------
    /// Example
    /// 
    /// Consider TCfg named RootCfg with two properties:
    /// - string foo
    /// - ChildProjectCfg
    ///
    /// In such case two config files are expected to be found:
    /// Root_config.json
    /// ChildProject_config.json
    ///
    /// where "foo" is property in Root_config.json and where
    /// RootCfg.ChildProjectCfg value will be deserialized from
    /// ChildProject_config.json.
    ///
    /// ----------------------------------------
    /// See also
    /// 
    /// For more executable examples, please see tests of this method.
    /// 
    /// </summary>
    public TCfg Read<TCfg>() where TCfg : IConfiguration
    {
        var (rootCfgFilePath, propCfgs) = ConfigFilePaths<TCfg>();
        JsonElement mergedJson = FS.ReadAllJson(rootCfgFilePath);
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