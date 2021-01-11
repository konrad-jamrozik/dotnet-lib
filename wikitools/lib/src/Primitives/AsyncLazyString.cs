namespace Wikitools.Lib.Primitives
{
    internal class AsyncLazyString : AsyncLazy<string>
    {
        public AsyncLazyString(string value) : base(() => value) {}
    }
}
