using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Wikitools.Lib.Json;
using Xunit;
using Xunit.Sdk;

namespace Wikitools.Lib.Tests.Json
{
    public class JsonDiffTests
    {
        private static readonly List<(string baseline, string target, string expectedDiff)> SimpleCases = new()
        {
            // @formatter:off
            ("1"          , "1"          , "{}"),
            ("1"          , "2"          , "'! baseline: 1 | target: 2'"),
            ("{}"         , "{}"         , "{}"),
            ("[]"         , "[]"         , "{}"),
            ("{}"         , "[]"         , "'! ValueKind baseline: Object ({}) | target: Array ([])'"),
            ("[]"         , "{}"         , "'! ValueKind baseline: Array ([]) | target: Object ({})'"),
            ("{}"         , "{ 'a': 1 }" , "{'a':'+'}"),
            ("{ 'b': 2 }" , "{}"         , "{'b':'-'}"),
            ("{ 'a': 2 }" , "{ 'b': 3 }" , "{'b':'+','a':'-'}"),
            ("[]"         , "[1]"        , "{'0':'+'}"),
            ("['a']"      , "[]"         , "{'0':'-'}"),
            ("['abc']"    , "['def']"    , "{'0':'! baseline: abc | target: def'}"),
            ("1"          , "[]"         , "'! ValueKind baseline: Number (1) | target: Array ([])'"),
            
            ("{ 'c': 2 }"         , "{ 'c': 'x' }"       , "{'c':'! ValueKind baseline: Number (2) | target: String (x)'}"),
            ("{ 'd': {} }"        , "{ 'd': [] }"        , "{'d':'! ValueKind baseline: Object ({}) | target: Array ([])'}"),
            ("{ 'e': 'xyz' }"     , "{ 'e': 'xyw' }"     , "{'e':'! baseline: xyz | target: xyw'}"),
            ("{ 'a': 1, 'b': 2 }" , "{ 'b': 2, 'a': 1 }" , "{}"),

            // @formatter:on
        };

        private static readonly List<(string baseline, string target, string expectedDiff)> ComplexCases = new()
        {
            // @formatter:off
            ("[1,'a']","['a',1]","{"+
                "'0':'! ValueKind baseline: Number (1) | target: String (a)',"+
                "'1':'! ValueKind baseline: String (a) | target: Number (1)'"+
                "}"),
            ("[1,2,3]","['a','b',3,'c']",
                "{"+
                "'0':'! ValueKind baseline: Number (1) | target: String (a)',"+
                "'1':'! ValueKind baseline: Number (2) | target: String (b)',"+
                "'3':'+'"+
                "}")

            // kja add deep nested cases: object in array in object, etc.
            // @formatter:on
        };

        [Fact]
        public void DiffIsEmptyTrue() => Assert.True(new JsonDiff("{}", "{}").IsEmpty);

        [Fact]
        public void DiffIsEmptyFalse() => Assert.False(new JsonDiff("{}", "[]").IsEmpty);

        [Fact]
        public void DiffsSimpleCases() => Verify(SimpleCases);

        [Fact]
        public void DiffsComplexCases() => Verify(ComplexCases);

        private static void Verify(List<(string baseline, string target, string expectedDiff)> testsData) =>
            Enumerable.Range(0, testsData.Count)
                .ToList()
                .ForEach(i => { Verify(i + 1, testsData[i]); });

        private static void Verify(int testIndex, (string baseline, string target, string expectedDiff) testData)
        {
            var baseline     = testData.baseline.Replace('\'', '"');
            var target       = testData.target.Replace('\'', '"');
            var expectedDiff = testData.expectedDiff.Replace('\'', '"');

            // Act
            var actualDiff = new JsonDiff(
                JsonDocument.Parse(baseline),
                JsonDocument.Parse(target)).ToRawString();

            // Assert
            if (expectedDiff != actualDiff)
            {
                var exceptionMessage = $"Test no {testIndex} failed." + Environment.NewLine +
                                       $"Baseline     : {baseline}" + Environment.NewLine +
                                       $"Target       : {target}" + Environment.NewLine +
                                       $"Expected diff: {expectedDiff}." + Environment.NewLine +
                                       $"Actual   diff: {actualDiff}";
                exceptionMessage = exceptionMessage.Replace('"', '\'');
                throw new XunitException(exceptionMessage);
            }
        }
    }
}