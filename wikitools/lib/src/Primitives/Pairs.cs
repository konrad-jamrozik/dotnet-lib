using System;

namespace Wikitools.Lib.Primitives
{
    internal class Pairs
    {
        private readonly Lazy<string[]> _pairs;

        internal Pairs(string[] pairs) : this(new Lazy<string[]>(() => pairs)) {}

        private Pairs(Lazy<string[]> pairs) { _pairs = pairs; }

        public string Value(string key)
        {
            int index = Array.FindIndex(_pairs.Value, cand => cand.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (index == -1) throw new InvalidOperationException($"No value found for key: {key}");

            return _pairs.Value[index + 1];
        }
    }
}
