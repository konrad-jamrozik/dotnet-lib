using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void ProcessSaveFile()
        {
            var fs = new FileSystem();
            var (inputXcfSave, outputDir, outputSoldiersFileName, outputItemCountsFileName) =
                new Configuration(fs).Load<IOxceCfg>(
                    configProjectName: "oxce-configs",
                    loadedClassNamespace: "Oxce.Configs");

            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var bases = Bases.FromSaveFile(yamlMapping);

            var soldiersOutputPath = Path.Join(outputDir, outputSoldiersFileName);
            var itemCountsOutputPath = Path.Join(outputDir, outputItemCountsFileName);

            WriteBaseSoldiers(bases.Soldiers.ToList(), soldiersOutputPath);
            WriteBaseItemCounts(bases.ItemCounts.ToList(), itemCountsOutputPath);
        }

        private static void WriteBaseSoldiers(
            List<Soldier> soldiers,
            string soldiersOutputPath)
        {
            string[] csvLines = Soldier.CsvHeaders().InList()
                .Concat(soldiers.OrderBy(s => s.Id).Select(s => s.CsvString())).ToArray();

            File.WriteAllLines(soldiersOutputPath, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        private static void WriteBaseItemCounts(
            List<ItemCount> itemCounts,
            string itemCountsOutputPath)
        {
            string[] csvLines = ItemCount.CsvHeaders().InList()
                .Concat(itemCounts.Select(s => s.CsvString())).ToArray();

            File.WriteAllLines(itemCountsOutputPath, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }
    }
}