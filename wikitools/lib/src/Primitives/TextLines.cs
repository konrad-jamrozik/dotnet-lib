using System;

namespace Wikitools.Lib.Primitives
{
    public class TextLines
    {
        private readonly string _value;

        public TextLines(string value) => _value = value;

        public string[] Value => _value.Split(Environment.NewLine);
    }
}