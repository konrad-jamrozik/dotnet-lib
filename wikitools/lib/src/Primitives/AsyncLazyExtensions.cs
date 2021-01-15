using System;

namespace Wikitools.Lib.Primitives
{
    public static class AsyncLazyExtensions
    {
        public static AsyncLazy<T> AsyncLazy<T>(this T target) => new(() => target);

        public static AsyncLazy<T> AsyncLazy<T>(this AsyncLazy<T> target, Func<T, T> f) => new(async () => f(await target.Value));
    }
}