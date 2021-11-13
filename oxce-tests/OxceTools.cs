using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

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
            var (inputXcfSave, outputDirectory, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();
            var basesNodesLines = GetBasesNodesLines(inputXcfSave);
            var soldiers = ParseSoldiers(basesNodesLines);
            WriteOutSoldiers(soldiers, outputDirectory);
        }

        private static IEnumerable<IEnumerable<string>> GetBasesNodesLines(string inputXcfSave)
        {
            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var basesLines = yamlMapping.Lines("bases").ToList();
            var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
            return basesNodesLines;
        }

        private static List<Soldier> ParseSoldiers(IEnumerable<IEnumerable<string>> basesLines)
        {
            var soldiers = basesLines.SelectMany(
                baseLines =>
                {
                    var (baseName, soldiersLines) = ParseBase(baseLines);
                    var soldiers = soldiersLines.Select(soldierLines => ParseSoldier(soldierLines, baseName));
                    return soldiers;
                }).ToList();
            return soldiers;
        }

        private static (string baseName, IEnumerable<IEnumerable<string>> soldiersLines) ParseBase(IEnumerable<string> baseLines)
        {
            var baseYaml = new YamlMapping(baseLines);
            var soldiersYaml = new YamlBlockSequence(baseYaml.Lines("soldiers"));
            var soldiersLines = soldiersYaml.NodesLines();
            return (baseName: ParseString(baseYaml, "name"), soldiersLines);
        }

        private static void WriteOutSoldiers(List<Soldier> soldiers, string outputDirectory)
        {
            string[] csvLines = Soldier.CsvHeaders().InList().Concat(soldiers.Select(s => s.CsvString())).ToArray();
            var soldierDataOutputFile = Path.Join(outputDirectory, "soldier_data.csv");

            File.WriteAllLines(soldierDataOutputFile, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        // kja curr work
        private static Soldier ParseSoldier(IEnumerable<string> soldierNodeLines, string baseName)
        {
            var soldier = new YamlMapping(soldierNodeLines);
            var type = ParseString(soldier, "type");
            var name = ParseString(soldier, "name");
            var missions = ParseInt(soldier, "missions");
            var kills = ParseInt(soldier, "kills");
            var recovery = ParseIntOrZero(soldier, "recovery");
            var manaMissing = ParseIntOrZero(soldier, "manaMissing");
            var rank = ParseInt(soldier, "rank");
            var soldierDiary = new YamlMapping(soldier.Lines("diary"));
            int monthsService = ParseIntOrZero(soldierDiary, "monthsService");
            int statGainTotal = ParseIntOrZero(soldierDiary, "statGainTotal");
            var initialStats = new YamlMapping(soldier.Lines("initialStats"));
            var currentStats = new YamlMapping(soldier.Lines("currentStats"));
            var currentTU = ParseInt(currentStats, "tu");
            var currentStamina = ParseInt(currentStats, "stamina");
            var currentHealth = ParseInt(currentStats, "health");
            var currentBravery = ParseInt(currentStats, "bravery");
            var currentReactions = ParseInt(currentStats, "reactions");
            var currentFiring = ParseInt(currentStats, "firing");
            var currentThrowing = ParseInt(currentStats, "throwing");
            var currentStrength = ParseInt(currentStats, "strength");
            var currentPsiStrength = ParseInt(currentStats, "psiStrength");
            var currentPsiSkill = ParseInt(currentStats, "psiSkill");
            var currentMelee = ParseInt(currentStats, "melee");
            var currentMana = ParseInt(currentStats, "mana");

            return new Soldier(
                name,
                type,
                baseName,
                missions,
                kills,
                rank,
                monthsService,
                recovery,
                manaMissing,
                statGainTotal,
                currentTU,
                currentStamina,
                currentHealth,
                currentBravery,
                currentReactions,
                currentFiring,
                currentThrowing,
                currentStrength,
                currentPsiStrength,
                currentPsiSkill,
                currentMelee,
                currentMana);
        }


        private static string ParseString(YamlMapping mapping, string key) 
            => mapping.Lines(key).Single();

        private static int ParseInt(YamlMapping mapping, string key)
            => int.Parse(mapping.Lines(key).Single());

        private static int ParseIntOrZero(YamlMapping mapping, string key)
            => int.TryParse(mapping.Lines(key).SingleOrDefault(), out var value) ? value : 0;


        [Test]
        public void ProcessSaveFileStoresYamlStub()
        {
            var (inputXcfSave, outputDirectory, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();

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
            var (inputXcfSave, outputDirectory, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();

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