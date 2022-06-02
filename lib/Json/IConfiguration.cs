using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

// kj2 why this is in Json namespace?
public interface IConfiguration
{
    IFileSystem FileSystem();
}