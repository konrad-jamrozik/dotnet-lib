using System.Collections.Generic;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    [TestFixture]
    public class ConfigurationTests
    {
        private record EmptyCfg : IConfiguration;

        private record LeafCfg(string Foo, int[] BarArr) : IConfiguration;

        private record CompositeCfg(string Qux, LeafCfg? LeafCfg) : IConfiguration;

        [Test]
        public void ReadsEmptyConfig() => Verify(new EmptyCfg());

        [Test]
        public void ReadsLeafConfig() => Verify(new LeafCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ReadsCompositeConfig() =>
            Verify(new CompositeCfg("fooVal", new LeafCfg("quxVal", new[] { 1, 2 })));

        [Test]
        public void ComposesAndReadsCompositeConfig()
        {
            var leafCfg           = new LeafCfg("fooVal", new[] { 1, 2 });
            var compositeCfg      = new CompositeCfg("quxVal", leafCfg);
            var compositeCfgShell = new CompositeCfg("quxVal", null);
            
            // kja 8 curr TDD for 7 The composite config shall be composed from two files
            // Pass key value pairs denoting the two config files with simpleCfg and compositeCfg. Their names and contents.
            var cfgs = new Dictionary<string, IConfiguration>
            {
                [IConfiguration.FileName(nameof(LeafCfg))] = leafCfg,
                [IConfiguration.FileName(nameof(CompositeCfg))] = compositeCfgShell
            };
            Verify2(cfgs, compositeCfg);
        }

        private void Verify2<TConfig>(Dictionary<string, IConfiguration> cfgs, TConfig expectedCfg) where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            foreach (var (cfgFileName, cfgVal) in cfgs)
            {
                fs.CurrentDir.WriteAllTextAsync(cfgFileName, cfgVal.ToJsonIndentedUnsafe());    
            }

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
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