using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Oxce.Configs;

public interface IOxceCfg : IConfiguration
{
    public string InputXcfSave();
    public string OutputDir();
    public string SoldiersOutputFileName();
    public string ItemCountsOutputFileName();

    public string SoldiersOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), SoldiersOutputFileName());

    public string ItemCountsOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), ItemCountsOutputFileName());

    public void Deconstruct(
        out string inputXcfSave,
        out string outputDirectory,
        out string soldiersOutputFileName,
        out string itemCountsOutputFileName)
    {
        inputXcfSave = InputXcfSave();
        outputDirectory = OutputDir();
        soldiersOutputFileName = SoldiersOutputFileName();
        itemCountsOutputFileName = ItemCountsOutputFileName();
    }
}