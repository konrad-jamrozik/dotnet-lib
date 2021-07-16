using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using MoreLinq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Tests.Json
{
    // kja 7 curr TDD for 8: tests for Read() without params, including reading multiple configs.
    // Remove obsolete logic and tests for it ("Old") when done with the new one.
    [TestFixture]
    public class ConfigurationTests
    {
        private record EmptyCfg : IConfiguration;

        private record LeafCfg(string Foo, int[] BarArr) : IConfiguration;

        private record CompositeCfg(string Qux, LeafCfg? LeafCfg) : IConfiguration;

        [Test]
        public void TypeReflectionScratchpad()
        {
            ReflectionTest<CompositeCfg>();
        }

        public void ReflectionTest<TCfg>() where TCfg : IConfiguration
        {
            foreach (var info in typeof(TCfg).GetProperties())
            {
                if (info.Name.EndsWith(IConfiguration.ConfigSuffix))
                {
                    Console.Out.WriteLine(
                        $"{info.Name} {info.MemberType} {info.PropertyType} {IConfiguration.FileName(info.PropertyType)}");
                }
            }
        }

        [Test] public void ReadsEmptyConfigNew() => VerifyNewReadSingle(new EmptyCfg());

        [Test] public void ReadsLeafConfigNew() => VerifyNewReadSingle(new LeafCfg("fooVal", new[] { 1, 2 }));

        [Test]
        public void ComposesAndReadsCompositeConfigNew()
        {
            var leafCfg           = new LeafCfg("fooVal", new[] { 1, 2 });
            var compositeCfg      = new CompositeCfg("quxVal", leafCfg);
            // The composite config, but with no *Cfg properties yet populated.
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

            cfgs.ForEach(cfg => fs.CurrentDir.WriteAllTextAsync(cfg.Key, cfg.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).ReadNew<TConfig>();

            new JsonDiffAssertion(expectedCfg, actualCfg).Assert();
        }

        private void VerifyNewRead<TConfig>(Dictionary<string, IConfiguration> cfgs, TConfig expectedCfg)
            where TConfig : IConfiguration
        {
            var fs = new SimulatedFileSystem();

            cfgs.ForEach(kvp => fs.CurrentDir.WriteAllTextAsync(kvp.Key, kvp.Value.ToJsonUnsafe(ignoreNulls: true)));

            // Act
            TConfig actualCfg = new Configuration(fs).ReadNew<TConfig>();

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