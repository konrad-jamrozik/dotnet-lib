using Wikitools.Lib.Json;

namespace Oxce.Configs;

public interface IOxceCfg : IConfiguration
{
    public string InputXcfSave();
    public string OutputDirectory();

    public void Deconstruct(
        out string inputXcfSave,
        out string outputDirectory)
    {
        inputXcfSave = InputXcfSave();
        outputDirectory = OutputDirectory();
    }
}