using Wikitools.Lib.OS;

namespace Wikitools.Lib.Json;

// kja why this is in Json namespace?
public interface IConfiguration
{
    IFileSystem FileSystem();
}