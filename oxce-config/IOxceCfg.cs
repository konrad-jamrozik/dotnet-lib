using Wikitools.Lib.Json;

namespace Oxce.Configs;

public interface IOxceCfg : IConfiguration
{
    public string InputXcfSave();
    public string OutputDirectory();
    public string SoldierDataOutputFileName();
    public string ItemCountsDataOutputFileName();

    public void Deconstruct(
        out string inputXcfSave,
        out string outputDirectory,
        out string soldierDataOutputFileName,
        out string itemCountsDataOutputFileName)
    {
        inputXcfSave = InputXcfSave();
        outputDirectory = OutputDirectory();
        soldierDataOutputFileName = SoldierDataOutputFileName();
        itemCountsDataOutputFileName = ItemCountsDataOutputFileName();
    }
}