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

            
            var headerRow = "itemType," + string.Join(",", basesItems.Select(baseItems => baseItems.Name));

            var itemTypes = basesItems.SelectMany(baseItems => baseItems.Items.Keys).ToHashSet();
            var itemRows = itemTypes.OrderBy(itemType => itemType).Select(itemType => ItemRow(itemType, basesItems));
            
            File.AppendAllLines(outputFile, new List<string> {headerRow});
            File.AppendAllLines(outputFile, itemRows);
        }

        private string ItemRow(string itemType, List<BaseItems> basesItems)
        {
            var counts = basesItems.Select(
                baseItems => baseItems.Items.ContainsKey(itemType) ? baseItems.Items[itemType] : 0);
            return itemType + "," + string.Join(",", counts);
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

        private static (string name, IEnumerable<KeyValuePair<string, int>> items) RegexMatchNameAndItems(List<string> nameLine, List<string> itemLines)
        {
            var name = Regex.Match(nameLine[0], "\\s+name:\\s(.*)").Groups[1].Value;

            var items = itemLines.Select(
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