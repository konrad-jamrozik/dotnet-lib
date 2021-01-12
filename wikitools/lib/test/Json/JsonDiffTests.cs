using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Wikitools.Lib.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Wikitools.Lib.Tests.Json
{
    public class JsonDiffTests
    {
        [Fact]
        public void DiffIsEmptyTrue() => Assert.True(new JsonDiff("{}", "{}").IsEmpty);

        [Fact]
        public void DiffIsEmptyFalse() => Assert.False(new JsonDiff("{}", "[]").IsEmpty);

        // kja write more tests
        [Fact]
        public void DiffsSimpleCases()
        {
            var testsData = new List<(string baseline, string target, string expectedDiff)>
            {
                // @formatter:off
                ("1"          , "1"          , "{}"),
                ("1"          , "2"          , "'! baseline: 1 | target: 2'"),
                ("{}"         , "{}"         , "{}"),
                ("[]"         , "[]"         , "{}"),
                ("{}"         , "[]"         , "'! ValueKind mismatch. baseline: Object ({}) | target: Array ([])'"),
                ("[]"         , "{}"         , "'! ValueKind mismatch. baseline: Array ([]) | target: Object ({})'"),
                ("{}"         , "{ 'a': 1 }" , "{'a':'+'}"),
                ("{ 'b': 2 }" , "{}"         , "{'b':'-'}"),
                ("{ 'a': 2 }" , "{ 'b': 3 }" , "{'b':'+','a':'-'}"),

                ("{ 'c': 2 }"    , "{ 'c': 'x' }"   , "{'c':'! ValueKind mismatch. baseline: Number (2) | target: String (x)'}"),
                ("{ 'd': {} }"   , "{ 'd': [] }"    , "{'d':'! ValueKind mismatch. baseline: Object ({}) | target: Array ([])'}"),
                ("{ 'e': 'xyz' }", "{ 'e': 'xyw' }" , "{'e':'! baseline: xyz | target: xyw'}")
                // @formatter:on
            };
            Enumerable.Range(0, testsData.Count)
                .ToList()
                .ForEach(i => { Verify(i + 1, testsData[i]); });
        }

        private static void Verify(int testIndex, (string baseline, string target, string expectedDiff) data)
        {
            var baseline     = data.baseline.Replace('\'', '"');
            var target       = data.target.Replace('\'', '"');
            var expectedDiff = data.expectedDiff.Replace('\'', '"');

            // Act
            var actualDiff = new JsonDiff(
                JsonDocument.Parse(baseline),
                JsonDocument.Parse(target)).ToRawString();

            // Assert
            if (expectedDiff != actualDiff)
            {
                var exceptionMessage = $"Test no {testIndex} failed." + Environment.NewLine +
                                       $"Expected diff: {expectedDiff}." + Environment.NewLine +
                                       $"Actual   diff: {actualDiff}";
                exceptionMessage = exceptionMessage.Replace('"', '\'');
                throw new XunitException(exceptionMessage);
            }
        }
    }
}