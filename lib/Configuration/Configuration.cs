using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Configuration;

public record Configuration(IFileSystem FileSystem)
{
    private const string ConfigRepoCloneDirPath = "dotnet-lib-private";

    private const string ConfigProjectName = "wikitools-configs";

    private const string NetFrameworkVersion = "net6.0";

    // The traversed directories are assumed to be:
    // \<repo-dir>\<project-dir>\bin\<configuration>\<net-version>
    private const string RelativeRepoPath = @"..\..\..\..\..";

    private string DllToLoadPath(string configProjectName) 
        => $@"{ConfigRepoCloneDirPath}\{configProjectName}\bin\Debug\"
           + $@"{NetFrameworkVersion}\{configProjectName}.dll";

    private const string LoadedTypeNamespace = "Wikitools.Configs";

    /// <summary>
    /// Loads implementation of a configuration interface TCfg from a C# assembly.
    ///
    /// Given TCfg interface named IFoo, this method will load class named Foo
    /// from a .dll file, call the class' no-param ctor, and return it.
    ///
    /// By default, the .dll file is expected to have a path of:
    ///
    ///   ./dotnet-lib-private/wikitools-configs/bin/Debug/net6.0/wikitools-configs.dll
    /// 
    /// where "." is a parent directory of local repo clone root dir, containing within its
    /// descendants .dll with this file.
    /// </summary>
    /// <remarks>
    /// Implementation inspired by
    /// https://stackoverflow.com/questions/465488/can-i-load-a-net-assembly-at-runtime-and-instantiate-a-type-knowing-only-the-na
    /// </remarks>
    public TCfg Load<TCfg>(
        string configProjectName = ConfigProjectName,
        string loadedTypeNamespace = LoadedTypeNamespace) where TCfg : IConfiguration
    {
        var assemblyWithTypeToLoad = AssemblyWithTypeToLoad(configProjectName);
        var nameOfTypeToLoad = LoadedTypeName<TCfg>(loadedTypeNamespace);
        Type type = assemblyWithTypeToLoad.GetType(nameOfTypeToLoad)!;
        TCfg cfg = (TCfg)Activator.CreateInstance(type, FileSystem)!;
        return cfg;
    }

    private Assembly AssemblyWithTypeToLoad(string configProjectName)
    {
        var currentDirectory = FileSystem.CurrentDir.Path;
        var dllPath = Path.Join(currentDirectory, RelativeRepoPath, DllToLoadPath(configProjectName));
        Assembly assemblyWithTypeToLoad = Assembly.LoadFrom(dllPath);
        return assemblyWithTypeToLoad;
    }

    private static string LoadedTypeName<TCfg>(string loadedTypeNamespace) where TCfg : IConfiguration
    {
        var interfaceName = typeof(TCfg).Name;
        var typeClassName = string.Concat(interfaceName.Skip(1)); // Remove the "I" from "IFoo".
        var loadedTypeName = $"{loadedTypeNamespace}.{typeClassName}";
        return loadedTypeName;
    }
}