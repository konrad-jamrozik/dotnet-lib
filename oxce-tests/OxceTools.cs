using System;
using System.IO;
using System.Linq;
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

            for (int i = 0; i < basesCount; i++)
            {
                var currBaseLines = remBasesLines
                    .Skip(1)
                    .TakeWhile(line => !line.StartsWith(FirstBaseLinePrefix))
                    .ToList();
                remBasesLines = remBasesLines.Skip(currBaseLines.Count + 1).ToList();
                Console.Out.WriteLine($"Appending {currBaseLines.Count} lines");
                File.AppendAllLines(outputFile, currBaseLines);
            }
        }
    }
}