using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

// kja use FS in the implementation
public record Configuration(IFileSystem FS)
{
    // Implementation inspired by
    // https://stackoverflow.com/questions/465488/can-i-load-a-net-assembly-at-runtime-and-instantiate-a-type-knowing-only-the-na
    // kja rethink the name of this metod
    public TCfg ReadFromAssembly<TCfg>() where TCfg : IConfiguration
    {
        // kja the hardcoded paths here should come from a ctor param, in turn
        // obtained from some top-level -config project, from a class
        // that has hardcoded/inline bootstrapping config denoting from which paths load the
        // remaining configs
        var currentDirectory = Directory.GetCurrentDirectory();
        // kja the traversalToRepoParent can be merged with drill-down path below. And then
        // the merged value should be passed as ctor param - see to-do above.
        // \dotnet-lib\wikitools-tests\bin\Debug\net6.0
        var traversalToRepoParent = @"..\..\..\..\..";
        var repoParentDir = Path.GetFullPath(Path.Join(currentDirectory, traversalToRepoParent));
        var dllPath = Path.Join(
            repoParentDir,
            @"dotnet-lib-private\wikitools-configs\bin\Debug\net6.0\wikitools-configs.dll");
        Assembly assembly = Assembly.LoadFrom(dllPath);
        var interfaceName = typeof(TCfg).Name;
        var typeClassName = string.Concat(interfaceName.Skip(1));
        Type type = assembly.GetType("Wikitools.Configs."+ typeClassName)!;

        TCfg cfg = (TCfg)Activator.CreateInstance(type)!;
        return cfg;
    }
}