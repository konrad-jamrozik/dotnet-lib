using System;
using System.Threading.Tasks;

namespace Wikitools.Lib.Primitives;

// ReSharper disable once UnusedType.Global
// - reason: keeping for now.
public static class AsyncLazyMonad
{
    public static AsyncLazy<T> AsAsyncLazy<T>(this Task<T> target) 
        => new(() => target);

    /// <summary>
    /// A slight variation on the monadic >>= (bind) operator for AsyncLazy, per [1]
    ///
    /// Signature:
    /// m a -> (a -> a) -> m a
    /// where:
    /// m:   AsyncLazy
    /// a:   typeof(await asyncLazyTarget.Value)
    ///      equivalently: typeof(target)
    /// m a: AsyncLazy typeof(await asyncLazyTarget.Value)
    /// (a -> a): typeof(f)
    ///
    /// If this would be an actual monadic bind operator, function f
    /// would have signature of (a -> m a)
    /// and would be instead implemented as f':
    /// f' = new AsyncLazy(() => f(target))
    /// 
    /// [1] https://www.haskell.org/tutorial/monads.html
    /// </summary>
    public static AsyncLazy<T> M<T>(
        this AsyncLazy<T> asyncLazyTarget,
        Func<T, T> f) => new AsyncLazy<T>(async () =>
    {
        T target = await asyncLazyTarget; // RHS is same as "await asyncLazyTarget.Value;"
        return f(target);
    });
}