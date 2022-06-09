using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Oxce.Configs;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.OS;

namespace OxceTests
{
    public class OxceTools
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task ProcessSaveFile()
        {
            await ParameterizedProcessSaveFile();
        }

        [Test]
        public async Task ProcessSaveFileOld()
        {
            await ParameterizedProcessSaveFile(
                "ufo landed.sav",
                "soldiers - ufo landed.csv",
                "item counts - ufo landed.csv");
        }

        [Test]
        public async Task ProcessSaveFileNew()
        {
            await ParameterizedProcessSaveFile(
                "ufo landed fixed reloaded.sav",
                "soldiers - ufo landed fixed reloaded.csv",
                "item counts - ufo landed fixed reloaded.csv");
        }



        private async Task ParameterizedProcessSaveFile(
            string saveFileName = null,
            string soldiersOutputFileName = null,
            string itemCountsOutputFileName = null)
        {
            var fs = new FileSystem();
            var cfg = new Configuration(fs).Load<IOxceCfg>(
                configProjectName: "oxce-configs",
                loadedTypeNamespace: "Oxce.Configs");

            var saveFilePath = saveFileName != null
                ? fs.JoinPath(cfg.SaveDir(), saveFileName)
                : cfg.SaveFilePath(fs);

            Console.Out.WriteLine("Reading " + saveFilePath);

            var soldiersOutputPath = soldiersOutputFileName != null
                ? fs.JoinPath(cfg.OutputDir(), soldiersOutputFileName)
                : cfg.SoldiersOutputPath(fs);

            var itemCountsOutputPath = itemCountsOutputFileName != null
                ? fs.JoinPath(cfg.OutputDir(), itemCountsOutputFileName)
                : cfg.ItemCountsOutputPath(fs);

            var yamlMapping = new YamlMapping(fs.ReadAllLines(saveFilePath));
            var bases = Bases.FromSaveFile(yamlMapping);

            await bases.WriteSoldiers(fs, soldiersOutputPath);
            await bases.WriteItemCounts(fs, itemCountsOutputPath);
        }
    }
}