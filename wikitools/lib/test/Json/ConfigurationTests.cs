using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    [TestFixture]
    public class ConfigurationTests
    {
        private class EmptyTestCfg : IConfiguration
        { }

        [Test]
        public void ReadsEmptyConfig()
        {
            var fs = new SimulatedFileSystem();
            fs.CurrentDir.WriteAllTextAsync("config.json", Lib.Json.Json.Empty);

            // Act
            new Configuration(fs).Read<EmptyTestCfg>("config.json");
        }

        [Test]
        public void ReadsSimpleConfig()
        {
            // kja 8 curr TDD for 7
            var fs = new SimulatedFileSystem();
            fs.CurrentDir.WriteAllTextAsync("config.json", Lib.Json.Json.Empty);
            var obj = new Configuration(fs).Read<EmptyTestCfg>("config.json");
        }
    }
}