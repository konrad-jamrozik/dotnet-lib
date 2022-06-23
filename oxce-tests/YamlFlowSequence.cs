using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;

namespace OxceTests;

// Limited parser of a flow sequence from https://yaml.org/spec/1.2.2/
public class YamlFlowSequence
{
    private readonly IEnumerable<string> _lines;

    public YamlFlowSequence(IEnumerable<string> lines)
    {
        _lines = lines;
        Contract.Assert(_lines.Count() <= 1);
    }

    public string[] ToArray()
        => _lines.Any()
            ? _lines.First().Trim('[', ']').Split(",").Select(element => element.Trim())
                .ToArray()
            : new string[] { };
}