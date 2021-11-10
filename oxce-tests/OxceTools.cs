using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace OxceTests
{
    public class OxceTools
    {
        private const string FirstBaseLinePrefix = "  - lon: ";




        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void ProcessSaveFileSoldiers()
        {
            var (inputXcfSave, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();
            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var basesLines = yamlMapping.Lines("bases").ToList();
            var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
            foreach (IEnumerable<string> baseNodeLines in basesNodesLines)
            {
                Console.Out.WriteLine("===== Next base! =====");
                var baseYamlMapping = new YamlMapping(baseNodeLines);
                var baseName = baseYamlMapping.Lines("name");
                Console.Out.WriteLine($"base name: {baseName.Single()}");
                var soldiersYamlBlockSequence = new YamlBlockSequence(baseYamlMapping.Lines("soldiers"));
                var soldiersNodesLines = soldiersYamlBlockSequence.NodesLines();
                foreach (IEnumerable<string> soldierNodeLines in soldiersNodesLines)
                {
                    Console.Out.WriteLine("    ===== Next soldier! =====");
                    var soldierYamlMapping = new YamlMapping(soldierNodeLines);
                    var soldierName = soldierYamlMapping.Lines("name");
                    Console.Out.WriteLine($"    soldier name: {soldierName.Single()}");
                }
            }
        }

        [Test]
        public void ProcessSaveFileStoresYamlStub()
        {
            var (inputXcfSave, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();

            var yaml = new Yaml(File.ReadAllLines(inputXcfSave));
            var basesSeq = yaml.BlockSequence("bases").ToList();
            var basesNames = basesSeq.Select(@base => @base.Scalar("name")).ToList();
            var basesItems = basesSeq.Select(@base => @base.MappingOfInts("items")).ToList();
            
            var headerRow = "itemName," + string.Join(",", basesNames);

            var itemsNames = basesItems
                .SelectMany(baseItems => baseItems.Keys)
                .ToHashSet();
            var itemsRows = itemsNames
                .OrderBy(itemName => itemName)
                .Select(itemName => ItemRow2(itemName, basesItems));
            
            File.WriteAllText(outputFile, ""); // Clear the output file
            File.AppendAllLines(outputFile, new List<string> {headerRow});
            File.AppendAllLines(outputFile, itemsRows);
        }

        private string ItemRow2(string itemName, List<Dictionary<string, int>> basesItems)
        {
            var counts = basesItems.Select(
                baseItems => baseItems.ContainsKey(itemName) ? baseItems[itemName] : 0);
            return itemName + "," + string.Join(",", counts);
        }


        [Test]
        public void ProcessSaveFile()
        {
            var (inputXcfSave, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();

            // Clear the output file
            File.WriteAllText(outputFile, "");

            var lines = File.ReadAllLines(inputXcfSave);
            Console.Out.WriteLine(lines.Length);

            var remBasesLines = lines
                .SkipWhile(line => line != "bases:")
                .Skip(1)
                .TakeWhile(line => line.StartsWith(" "))
                .ToList();

            int basesCount = remBasesLines.Count(line => line.StartsWith(FirstBaseLinePrefix));
            List<BaseItems> basesItems = new List<BaseItems>(basesCount);
            
            for (int i = 0; i < basesCount; i++)
            {
                var currBaseLines = CurrBaseLines(ref remBasesLines);

                var (nameLine, itemsLines) = ReadNameAndItemsLines(currBaseLines);
                var (name, items) = RegexMatchNameAndItems(nameLine, itemsLines);
                var baseItems = new BaseItems(name, new Dictionary<string, int>(items));
                basesItems.Add(baseItems);

                Console.Out.WriteLine($"Appending {itemsLines.Count + 1} lines");
                File.AppendAllLines(outputFile, nameLine);
                File.AppendAllLines(outputFile, itemsLines);
            }

            var headerRow = "itemName," + string.Join(",", basesItems.Select(baseItems => baseItems.Name));

            var itemNames = basesItems
                .SelectMany(baseItems => baseItems.Items.Keys)
                .ToHashSet();
            var itemRows = itemNames
                .OrderBy(itemName => itemName)
                .Select(itemName => ItemRow(itemName, basesItems));
            
            File.AppendAllLines(outputFile, new List<string> {headerRow});
            File.AppendAllLines(outputFile, itemRows);
        }

        private string ItemRow(string itemName, List<BaseItems> basesItems)
        {
            var counts = basesItems.Select(
                baseItems => baseItems.Items.ContainsKey(itemName) ? baseItems.Items[itemName] : 0);
            return itemName + "," + string.Join(",", counts);
        }

        private static List<string> CurrBaseLines(ref List<string> remBasesLines)
        {
            var currBaseLines = remBasesLines
                .Skip(1)
                .TakeWhile(line => !line.StartsWith(FirstBaseLinePrefix))
                .ToList();
            remBasesLines = remBasesLines.Skip(currBaseLines.Count + 1).ToList();
            return currBaseLines;
        }

        private static (List<string> nameLine, List<string> itemLines) ReadNameAndItemsLines(List<string> currBaseLines)
        {
            currBaseLines = currBaseLines.SkipWhile(line => !line.Contains("    name: ")).ToList();
            var nameLine = currBaseLines.Take(1).ToList();
            currBaseLines = currBaseLines.SkipWhile(line => !line.StartsWith("    items:")).ToList();
            var itemLines = currBaseLines.Skip(1).TakeWhile(line => line.StartsWith("      ")).ToList();
            return (nameLine, itemLines);
        }

        private static (string name, IEnumerable<KeyValuePair<string, int>> items) RegexMatchNameAndItems(List<string> nameLine, List<string> itemsLines)
        {
            var name = Regex.Match(nameLine[0], "\\s+name:\\s(.*)").Groups[1].Value;

            if (itemsLines.All(line => line == "      {}"))
                return (name, new KeyValuePair<string, int>[0]);

            var items = itemsLines.Select(
                itemLine =>
                {
                    var match = Regex.Match(itemLine, "\\s+(\\w+):\\s(\\d+)");
                    var itemName = match.Groups[1].Value;
                    var itemCount = int.Parse(match.Groups[2].Value);
                    return new KeyValuePair<string, int>(itemName, itemCount);
                });
            return (name, items);
        }
    }
}