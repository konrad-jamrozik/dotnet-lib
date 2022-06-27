using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.OS;

namespace OxceTests;

public record ParsedLines(List<string> Lines, (int start, int end) Offsets)
{
    public static ParsedLines FromFile(IFileSystem fs, string filePath)
    {
        var lines = fs.ReadAllLines(filePath);
        return new ParsedLines(lines.ToList(), (0, lines.Length));
    }
}