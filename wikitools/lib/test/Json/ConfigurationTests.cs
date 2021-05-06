using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    [TestFixture]
    public class ConfigurationTests
    {
        private record EmptyCfg : IConfiguration;

        private record SimpleCfg(string Foo, int[] BarArr) : IConfiguration;

        private record CompositeCfg(string Qux, SimpleCfg SimpleCfg) : IConfiguration;

        [Test]
        public void ReadsEmptyConfig() => Verify(new EmptyCfg());

        [Test]
        public void ReadsSimpleConfig() => Verify(new SimpleCfg("fooVal", new[] { 1, 2 }));

        // kja 8 curr TDD for 7 The composite config shall be composed from two files
        [Test]
        public void ReadsCompositeConfig() => Verify(new CompositeCfg("fooVal", new SimpleCfg("quxVal", new[] { 1, 2 })));

        private void Verify<T>(T cfg) where T : IConfiguration
        {
            var fs = new SimulatedFileSystem();
            fs.CurrentDir.WriteAllTextAsync("config.json", cfg.ToJsonIndentedUnsafe());

            // Act
            T obj = new Configuration(fs).Read<T>("config.json");

            new JsonDiffAssertion(cfg, obj).Assert();

        }
    }
}