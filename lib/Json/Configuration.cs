using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

public record Configuration(IFileSystem FS)
{
    private const string ConfigRepoCloneDirPath = "dotnet-lib-private";

    private const string ConfigProjectName = "wikitools-configs";

    private const string NetFrameworkVersion = "net6.0";

    // The traversed directories are assumed to be:
    // \<repo-dir>\<project-dir>\bin\<configuration>\<net-version>
    private const string RelativeRepoPath = @"..\..\..\..\..";

    private const string DllToLoadPath =
        $@"{ConfigRepoCloneDirPath}\{ConfigProjectName}\bin\Debug\"
        + $@"{NetFrameworkVersion}\{ConfigProjectName}.dll";

    private const string LoadedClassNamespace = "Wikitools.Configs.";

    // Implementation inspired by
    // https://stackoverflow.com/questions/465488/can-i-load-a-net-assembly-at-runtime-and-instantiate-a-type-knowing-only-the-na
    // kja rethink the name of this method
    public TCfg ReadFromAssembly<TCfg>() where TCfg : IConfiguration
    {
        var currentDirectory = FS.CurrentDir.Path;
        var dllPath = Path.Join(currentDirectory, RelativeRepoPath, DllToLoadPath);
        Assembly assembly = Assembly.LoadFrom(dllPath);
        var interfaceName = typeof(TCfg).Name;
        var typeClassName = string.Concat(interfaceName.Skip(1));
        Type type = assembly.GetType(LoadedClassNamespace+ typeClassName)!;
        TCfg cfg = (TCfg)Activator.CreateInstance(type)!;
        return cfg;
    }
}