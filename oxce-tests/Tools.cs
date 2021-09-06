using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace OxceTests
{
    public class Tools
    {
        private const string FirstBaseLinePrefix = " - lon: ";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
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
                var currBaseLines = remBasesLines.Take(1).TakeWhile(line => line != FirstBaseLinePrefix);
                remBasesLines = remBasesLines.Skip(currBaseLines.Count()).ToList();
            }

            File.WriteAllLines(outputFile, remBasesLines);
        }
    }
}