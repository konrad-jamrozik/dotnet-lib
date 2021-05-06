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

        private record NestedCfg(string Qux, SimpleCfg? SimpleCfg) : IConfiguration;

        [Test]
        public void ReadsEmptyConfig() => Verify(new EmptyCfg());

        [Test]
        public void ReadsSimpleConfig() => Verify(new SimpleCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ReadsNestedConfig() =>
            Verify(new NestedCfg("fooVal", new SimpleCfg("quxVal", new[] { 1, 2 })));

        [Test]
        public void ReadsCompositeConfig()
        {
            var simpleCfg            = new SimpleCfg("quxVal", new[] { 1, 2 });
            var compositeCfg         = new NestedCfg("fooVal", simpleCfg);
            var compositeCfgTemplate = new NestedCfg("fooVal", null);

            // kja 8 curr TDD for 7 The composite config shall be composed from two files
            // Pass key value pairs denoting the two config files with simpleCfg and compositeCfg. Their names and contents.
            Verify(compositeCfg);
        }

        private void Verify2<TConfig>(TConfig cfg) where TConfig : IConfiguration
        {
            var fs          = new SimulatedFileSystem();
            var cfgFileName = "config.json";

            fs.CurrentDir.WriteAllTextAsync(cfgFileName, cfg.ToJsonIndentedUnsafe());

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>(cfgFileName);

            new JsonDiffAssertion(cfg, actualCfg).Assert();
        }

        private void Verify<TConfig>(TConfig cfg) where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();
            var cfgFileName = "config.json";

            fs.CurrentDir.WriteAllTextAsync(cfgFileName, cfg.ToJsonUnsafe(ignoreNulls: true));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>(cfgFileName);

            new JsonDiffAssertion(cfg, actualCfg).Assert();
        }
    }
}