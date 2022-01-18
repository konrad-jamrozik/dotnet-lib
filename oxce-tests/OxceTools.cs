using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Oxce.Configs;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

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
            var (inputXcfSave, outputDir, outputSoldiersFileName, outputItemCountsFileName) =
                new Configuration(fs).Load<IOxceCfg>(
                    configProjectName: "oxce-configs",
                    loadedClassNamespace: "Oxce.Configs");

            var yamlMapping = new YamlMapping(fs.ReadAllLines(inputXcfSave));
            var bases = Bases.FromSaveFile(yamlMapping);

            var soldiersOutputPath = fs.JoinPath(outputDir, outputSoldiersFileName);
            var itemCountsOutputPath = fs.JoinPath(outputDir, outputItemCountsFileName);

            await bases.WriteSoldiers(fs, soldiersOutputPath);
            await bases.WriteItemCounts(fs, itemCountsOutputPath);
        }


    }
}