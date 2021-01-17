using System;

namespace Wikitools.Lib.Primitives
{
    // kja to remove when TabularData is removed
    public class TextLines
    {
        private readonly string _value;

        public TextLines(string value) => _value = value;

        public string[] Value => _value.Split(Environment.NewLine);
    }
}