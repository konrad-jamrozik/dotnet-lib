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
            var (inputXcfSave, outputDir, outputSoldiersFileName, outputItemCountsFileName) =
                new Configuration(new FileSystem()).Load<IOxceCfg>(
                    configProjectName: "oxce-configs",
                    loadedClassNamespace: "Oxce.Configs");

            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var bases = Bases.FromSaveFile(yamlMapping);

            WriteBaseSoldiers(bases.Soldiers.ToList(), outputDir, outputSoldiersFileName);
            WriteBaseItemCounts(bases.ItemCounts.ToList(), outputDir, outputItemCountsFileName);
        }

        private static void WriteBaseSoldiers(
            List<Soldier> soldiers,
            string outputDirectory,
            string outputSoldiersFileName)
        {
            string[] csvLines = Soldier.CsvHeaders().InList()
                .Concat(soldiers.OrderBy(s => s.Id).Select(s => s.CsvString())).ToArray();
            var soldierDataOutputFile = Path.Join(outputDirectory, outputSoldiersFileName);

            File.WriteAllLines(soldierDataOutputFile, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        private static void WriteBaseItemCounts(
            List<ItemCount> items,
            string outputDir,
            string outputItemCountsFileName)
        {
            string[] csvLines = ItemCount.CsvHeaders().InList()
                .Concat(items.Select(s => s.CsvString())).ToArray();
            var itemCountDataOutputFile = Path.Join(outputDir, outputItemCountsFileName);

            File.WriteAllLines(itemCountDataOutputFile, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }
    }
}