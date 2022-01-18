using System.Threading.Tasks;
using NUnit.Framework;
using Oxce.Configs;
using Wikitools.Lib.Json;
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
            var fs = new FileSystem();
            var cfg = new Configuration(fs).Load<IOxceCfg>(
                configProjectName: "oxce-configs",
                loadedClassNamespace: "Oxce.Configs");

            var yamlMapping = new YamlMapping(fs.ReadAllLines(cfg.InputXcfSave()));
            var bases = Bases.FromSaveFile(yamlMapping);

            await bases.WriteSoldiers(fs, cfg.SoldiersOutputPath(fs));
            await bases.WriteItemCounts(fs, cfg.ItemCountsOutputPath(fs));
        }


    }
}