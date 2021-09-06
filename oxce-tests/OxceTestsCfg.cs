using Wikitools.Lib.Json;

namespace OxceTests
{
    public record OxceTestsCfg(string InputXcfSave, string OutputFile) : IConfiguration;
}