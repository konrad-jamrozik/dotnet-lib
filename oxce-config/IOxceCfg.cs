using Wikitools.Lib.Json;

namespace Oxce.Config;

public interface IOxceCfg : IConfiguration
{
    public string InputXcfSave();
    public string OutputDirectory();
    public string OutputFile();

    public void Deconstruct(
        out string inputXcfSave,
        out string outputDirectory,
        out string outputFile)
    {
        inputXcfSave = InputXcfSave();
        outputDirectory = OutputDirectory();
        outputFile = OutputFile();
    }
}