using Wikitools.Lib.Json;

namespace OxceTests
{
    public record OxceTestsCfg(string InputXcfSave, string OutputDirectory, string OutputFile) : IConfiguration;
}