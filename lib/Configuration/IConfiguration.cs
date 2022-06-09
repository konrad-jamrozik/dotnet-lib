using Wikitools.Lib.OS;

namespace Wikitools.Lib.Configuration;

public interface IConfiguration
{
    IFileSystem FileSystem();
}