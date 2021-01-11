using System;

namespace Wikitools.Lib.Primitives
{
    internal class LazyBool : Lazy<bool>
    {
        private LazyBool(bool value) : base(() => value) {}

        public static implicit operator LazyBool(bool boolean) => new LazyBool(boolean);
    }
}
