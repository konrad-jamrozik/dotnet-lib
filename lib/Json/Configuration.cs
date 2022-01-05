using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

// kja need to figure out better way of handling configs, as they are not versioned.
// The config values are not strictly speaking secrets, but I cannot share them as this is in public
// repo yet the config values refer to company-internal artifacts.
// Idea: create private repo for configs, check it out beside dotnet-lib repo, and in publicly visible
// "meta-config", point to the path of the clone of the private repo having proper config.
//
// While at it, abandon the idea of having "default project" .json config files in the project dirs.
// Just load it from .dll:
// https://stackoverflow.com/questions/465488/can-i-load-a-net-assembly-at-runtime-and-instantiate-a-type-knowing-only-the-na

// kja problem with config refresh. They refresh in the /bin of owning project, but not in the runtime project, so rebuild doesn't refresh them properly.
// Specifically, after purging configs, first build seems to put all configs in the entry project /bin,
// but successive rebuilds do not update the configs for dependency projects.
public record Configuration(IFileSystem FS)
{
    /// <summary>
    /// Reads configuration TCfg from the file system FS.
    ///
    /// ----------------------------------------
    /// Algorithm:
    /// 
    /// The TCfg is deserialized from a series of .json configuration files.
    ///
    /// 1. The root config file is searched in Directory.GetCurrentDirectory().
    /// It searches for file named TCfg_config.json. If it won't find it, it tries, recursively,
    /// all parents of the current dir. It throws if the file is not found.
    /// 
    /// 2. All TCfg properties are read directly from the root config file found in step 1.
    /// into a new instance of TCfg, except TCfg properties that end with Cfg.
    /// 
    /// 3. For TCfg properties that end with Cfg, they are read from their own json files.
    /// The algorithm is analogous as in step 1. It also starts from current dir, but searches for
    /// files named PropCfg_config.json, where PropCfg is the name of the property.
    /// The algorithm used for the PropCfg_config.json files doesn't allow further nesting
    /// of configs.
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