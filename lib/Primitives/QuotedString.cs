namespace Wikitools.Lib.Primitives;

public class QuotedString
{
    private readonly string _value;
    public QuotedString(string value) => _value = value;

    public string Value => $"\"{_value}\"";
}