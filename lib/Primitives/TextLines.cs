using System;
using System.Linq;

namespace Wikitools.Lib.Primitives;

public class TextLines
{
    private readonly string _value;

    public TextLines(string value) => _value = value;

    public string[] Split => _value.Split(Environment.NewLine);

    public string[] SplitTrimmingEnd =>
        Split.Reverse().SkipWhile(string.IsNullOrWhiteSpace).Reverse().ToArray();
}