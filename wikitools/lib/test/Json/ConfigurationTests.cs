using System.Collections.Generic;
using MoreLinq;
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

        [Test] public void ReadsEmptyConfig() => VerifyReadSingle(new EmptyCfg());

        [Test] public void ReadsLeafConfig() => VerifyReadSingle(new LeafCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ReadsCompositeConfig()
        {
            var leafCfg           = new LeafCfg("fooVal", new[] { 1, 2 });
            var compositeCfg      = new CompositeCfg("quxVal", leafCfg);
            // The composite config, but with no *Cfg properties yet populated.
            // Needed to populate corresponding .json config file in the simulated file system.
            var compositeCfgShell = new CompositeCfg("quxVal", null);

            var cfgs = new Dictionary<string, IConfiguration>
            {
                [IConfiguration.FileName(typeof(LeafCfg))] = leafCfg,
                [IConfiguration.FileName(typeof(CompositeCfg))] = compositeCfgShell
            };
            VerifyRead(cfgs, compositeCfg);
        }

        private void VerifyReadSingle<TConfig>(TConfig expectedCfg) where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            var cfgs = new Dictionary<string, IConfiguration>
            {
                [IConfiguration.FileName(typeof(TConfig))] = expectedCfg
            };

            cfgs.ForEach(cfg => fs.CurrentDir.WriteAllTextAsync(cfg.Key, cfg.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
        }

        private void VerifyRead<TConfig>(Dictionary<string, IConfiguration> cfgs, TConfig expectedCfg)
            where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            cfgs.ForEach(kvp => fs.CurrentDir.WriteAllTextAsync(kvp.Key, kvp.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
        }
    }
}