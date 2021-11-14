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
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ProcessSaveFileBaseSoldiers()
        {
            var (inputXcfSave, outputDirectory, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();
            var basesLines = GetBasesLines(inputXcfSave);
            var soldiers = ParseBaseSoldiers(basesLines);
            WriteBaseSoldiers(soldiers, outputDirectory);
        }

        [Test]
        public void ProcessSaveFileBaseItemCounts()
        {
            var (inputXcfSave, outputDirectory, outputFile) = new Configuration(new FileSystem()).Read<OxceTestsCfg>();
            var basesNodesLines = GetBasesLines(inputXcfSave);
            var itemCounts = ParseBaseItemCounts(basesNodesLines);
            WriteBaseItemCounts(itemCounts, outputDirectory);
        }

        private static IEnumerable<IEnumerable<string>> GetBasesLines(string inputXcfSave)
        {
            var yamlMapping = new YamlMapping(File.ReadAllLines(inputXcfSave));
            var basesLines = yamlMapping.Lines("bases").ToList();
            var basesNodesLines = new YamlBlockSequence(basesLines).NodesLines();
            return basesNodesLines;
        }

        private static List<Soldier> ParseBaseSoldiers(IEnumerable<IEnumerable<string>> basesLines)
        {
            var soldiers = basesLines.SelectMany(
                baseLines =>
                {
                    var (baseName, soldiersLines) = ParseBaseSoldiers(baseLines);
                    var soldiers = soldiersLines.Select(soldierLines => ParseSoldier(soldierLines, baseName));
                    return soldiers;
                }).ToList();
            return soldiers;
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

        private static void WriteBaseSoldiers(List<Soldier> soldiers, string outputDirectory)
        {
            string[] csvLines = Soldier.CsvHeaders().InList().Concat(soldiers.Select(s => s.CsvString())).ToArray();
            var soldierDataOutputFile = Path.Join(outputDirectory, "base_soldiers_query.csv");

            File.WriteAllLines(soldierDataOutputFile, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        private static void WriteBaseItemCounts(List<ItemCount> items, string outputDirectory)
        {
            string[] csvLines = ItemCount.CsvHeaders().InList().Concat(items.Select(s => s.CsvString())).ToArray();
            var itemCountDataOutputFile = Path.Join(outputDirectory, "base_item_counts_query.csv");

            File.WriteAllLines(itemCountDataOutputFile, csvLines);
            csvLines.ForEach(line => Console.Out.WriteLine(line));
        }

        private static Soldier ParseSoldier(IEnumerable<string> soldierLines, string baseName)
        {
            var soldier = new YamlMapping(soldierLines);
            var type = ParseString(soldier, "type");
            var name = ParseString(soldier, "name");
            var missions = ParseInt(soldier, "missions");
            var kills = ParseInt(soldier, "kills");
            var recovery = ParseFloatOrZero(soldier, "recovery");
            var manaMissing = ParseIntOrZero(soldier, "manaMissing");
            var rank = ParseInt(soldier, "rank");
            var soldierDiary = new YamlMapping(soldier.Lines("diary"));
            var monthsService = ParseIntOrZero(soldierDiary, "monthsService");
            var statGainTotal = ParseIntOrZero(soldierDiary, "statGainTotal");
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

        private static float ParseFloatOrZero(YamlMapping mapping, string key)
            => float.TryParse(mapping.Lines(key).SingleOrDefault(), out var value) ? value : 0;
    }
}