using System;

namespace Wikitools.Lib.Primitives
{
    internal class LazyString : Lazy<string>
    {
        public LazyString(Func<string> func) : base(func.Invoke) {}
        public LazyString(Pairs pairs, string key) : base(() => pairs.Value(key)) {}

        private LazyString(string value) : base(() => value) { }

        public static implicit operator LazyString(string str) => new LazyString(str);
    }
}
