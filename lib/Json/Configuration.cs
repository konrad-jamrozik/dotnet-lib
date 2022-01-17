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

    private string DllToLoadPath(string configProjectName) =>
        $@"{ConfigRepoCloneDirPath}\{configProjectName}\bin\Debug\"
        + $@"{NetFrameworkVersion}\{configProjectName}.dll";

    private const string LoadedClassNamespace = "Wikitools.Configs";

    /// <summary>
    /// Loads implementation of a configuration interface TCfg from a C# assembly.
    ///
    /// Given TCfg interface named IFoo, this method will load class named Foo
    /// from a .dll file, call the class no-param ctor, and return it.
    /// Inspect the method implementation for details.
    /// </summary>
    /// <remarks>
    /// Implementation inspired by
    /// https://stackoverflow.com/questions/465488/can-i-load-a-net-assembly-at-runtime-and-instantiate-a-type-knowing-only-the-na
    /// </remarks>
    public TCfg Load<TCfg>(
        string configProjectName = ConfigProjectName,
        string loadedClassNamespace = LoadedClassNamespace) where TCfg : IConfiguration
    {
        var currentDirectory = FS.CurrentDir.Path;
        var dllPath = Path.Join(currentDirectory, RelativeRepoPath, DllToLoadPath(configProjectName));
        Assembly assembly = Assembly.LoadFrom(dllPath);
        var interfaceName = typeof(TCfg).Name;
        var typeClassName = string.Concat(interfaceName.Skip(1));
        Type type = assembly.GetType($"{loadedClassNamespace}.{typeClassName}")!;
        TCfg cfg = (TCfg)Activator.CreateInstance(type)!;
        return cfg;
    }
}