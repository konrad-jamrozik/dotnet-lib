using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    [TestFixture]
    public class ConfigurationTests
    {
        private class TestCfg : IConfiguration
        { }

        [Test]
        public void ReadsEmptyConfig()
        {
            // kja 8 curr TDD for 7
            var fs  = new SimulatedFileSystem();
            var obj = new Configuration(fs).Read<TestCfg>("config.json");
        }
    }
}