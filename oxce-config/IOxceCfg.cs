using Wikitools.Lib.Configuration;
using Wikitools.Lib.OS;

namespace Oxce.Configs;

public interface IOxceCfg : IConfiguration
{
    public string SaveDir();
    public string SaveFileName();
    public string OutputDir();
    public string SoldiersOutputFileName();
    public string ItemCountsOutputFileName();

    public string SaveFilePath(IFileSystem fs) => fs.JoinPath(SaveDir(), SaveFileName());

    public string SoldiersOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), SoldiersOutputFileName());

    public string ItemCountsOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), ItemCountsOutputFileName());
}