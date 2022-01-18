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
        public void ProcessSaveFileBaseSoldiers()
        {
            var (inputXcfSave, outputDir, outputSoldiersFileName, _) =
                new Configuration(new FileSystem()).Load<IOxceCfg>(
                    configProjectName: "oxce-configs",
                    loadedClassNamespace: "Oxce.Configs");

            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var bases = Bases.FromSaveGameYamlMapping(yamlMapping);

            WriteBaseSoldiers(bases.Soldiers.ToList(), outputDir, outputSoldiersFileName);
        }

        [Test]
        public void ProcessSaveFileBaseItemCounts()
        {
            var (inputXcfSave, outputDir, _, outputItemCountsFileName) =
                new Configuration(new FileSystem()).Load<IOxceCfg>(
                    configProjectName: "oxce-configs",
                    loadedClassNamespace: "Oxce.Configs");
            var basesNodesLines = GetBasesLines(inputXcfSave);
            var itemCounts = ParseBaseItemCounts(basesNodesLines);
            WriteBaseItemCounts(itemCounts, outputDir, outputItemCountsFileName);
        }

        private static IEnumerable<IEnumerable<string>> GetBasesLines(string inputXcfSave)
        {
            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var basesLines = yamlMapping.Lines("bases").ToList();
            var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
            return basesNodesLines;
        }

        private static List<ItemCount> ParseBaseItemCounts(IEnumerable<IEnumerable<string>> basesLines)
        {
            var items = basesLines.SelectMany(
                baseLines =>
                {
                    var (baseName, itemCountsDataPairs) = ParseBaseItemCounts(baseLines);
                    var itemCounts = itemCountsDataPairs.Select(pair => new ItemCount(baseName, pair.Key, int.Parse(pair.Value)));
                    return itemCounts;
                }).ToList();
            return items;
        }

        private static (string baseName, IEnumerable<IEnumerable<string>> soldiersLines) ParseBaseSoldiers(IEnumerable<string> baseLines)
        {
            var baseYaml = new YamlMapping(baseLines);
            var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
            var soldiersLines = soldiersYaml.NodesLines();
            return (baseName: ParseString(baseYaml, "name"), soldiersLines);
        }

        private static (string baseName, IEnumerable<(string Key, string Value)> itemCountsDataPairs) ParseBaseItemCounts(IEnumerable<string> baseLines)
        {
            var baseYaml = new YamlMapping(baseLines);
            var itemCountsYaml = new YamlMapping(baseYaml.Lines("items"));
            var itemCountsDataPairs = itemCountsYaml.KeyValuePairs();
            return (baseName: ParseString(baseYaml, "name"), itemCountsDataPairs);
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

        private static string ParseString(YamlMapping mapping, string key) 
            => mapping.Lines(key).Single();
    }
}