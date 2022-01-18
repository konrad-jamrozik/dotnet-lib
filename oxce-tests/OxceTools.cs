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

            await WriteBaseSoldiers(fs, bases.Soldiers.ToList(), soldiersOutputPath);
            await WriteBaseItemCounts(fs, bases.ItemCounts.ToList(), itemCountsOutputPath);
        }

        private static async Task WriteBaseSoldiers(
            IFileSystem fs,
            List<Soldier> soldiers,
            string soldiersOutputPath)
        {
            string[] csvLines = Soldier.CsvHeaders().InList()
                .Concat(soldiers.OrderBy(s => s.Id).Select(s => s.CsvString())).ToArray();

            await fs.WriteAllLinesAsync(soldiersOutputPath, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        private static async Task WriteBaseItemCounts(
            IFileSystem fs,
            List<ItemCount> itemCounts,
            string itemCountsOutputPath)
        {
            string[] csvLines = ItemCount.CsvHeaders().InList()
                .Concat(itemCounts.Select(s => s.CsvString())).ToArray();

            await fs.WriteAllLinesAsync(itemCountsOutputPath, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }
    }
}