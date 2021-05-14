using System.Collections.Generic;
using MoreLinq;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    // kja 8 curr TDD for 7: tests for Read() without params, including reading multiple configs.
    [TestFixture]
    public class ConfigurationTests
    {
        private record EmptyCfg : IConfiguration;

        private record LeafCfg(string Foo, int[] BarArr) : IConfiguration;

        private record CompositeCfg(string Qux, LeafCfg? LeafCfg) : IConfiguration;

        [Test] public void ReadsEmptyConfigNew() => VerifyNewReadSingle(new EmptyCfg());

        [Test] public void ReadsLeafConfigNew() => VerifyNewReadSingle(new LeafCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ReadsCompositeConfigNew() =>
            VerifyNewReadSingle(new CompositeCfg("fooVal", new LeafCfg("quxVal", new[] { 1, 2 })));

        [Test]
        public void ComposesAndReadsCompositeConfig()
        {
            var leafCfg           = new LeafCfg("fooVal", new[] { 1, 2 });
            var compositeCfg      = new CompositeCfg("quxVal", leafCfg);
            var compositeCfgShell = new CompositeCfg("quxVal", null);

            var cfgs = new Dictionary<string, IConfiguration>
            {
                [IConfiguration.FileName(typeof(LeafCfg))] = leafCfg,
                [IConfiguration.FileName(typeof(CompositeCfg))] = compositeCfgShell
            };
            VerifyNewRead(cfgs, compositeCfg);
        }

        [Test] public void ReadsEmptyConfig() => VerifyOldRead(new EmptyCfg());


        [Test] public void ReadsLeafConfig() => VerifyOldRead(new LeafCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ReadsCompositeConfig() =>
            VerifyOldRead(new CompositeCfg("fooVal", new LeafCfg("quxVal", new[] { 1, 2 })));


        private void VerifyNewReadSingle<TConfig>(TConfig expectedCfg) where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            var cfgs = new Dictionary<string, IConfiguration>
            {
                [IConfiguration.FileName(typeof(TConfig))] = expectedCfg
            };

            cfgs.ForEach(kvp => fs.CurrentDir.WriteAllTextAsync(kvp.Key, kvp.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
        }

        private void VerifyNewRead<TConfig>(Dictionary<string, IConfiguration> cfgs, TConfig expectedCfg)
            where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            cfgs.ForEach(kvp => fs.CurrentDir.WriteAllTextAsync(kvp.Key, kvp.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
        }

        private void VerifyOldRead<TConfig>(TConfig cfg) where TConfig : IConfiguration
        {
            var fs          = new SimulatedFileSystem();
            var cfgFileName = "config.json";

            fs.CurrentDir.WriteAllTextAsync(cfgFileName, cfg.ToJsonUnsafe(ignoreNulls: true));

            // Act
            TConfig actualCfg = new Configuration(fs).Read<TConfig>(cfgFileName);

            new JsonDiffAssertion(cfg, actualCfg).Assert();
        }
    }
}