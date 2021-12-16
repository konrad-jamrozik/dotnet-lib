using System;
using System.Threading.Tasks;

namespace Wikitools.Lib.Primitives;

public static class TaskExtensions
{
    /// <summary>
    /// The monadic >>= (bind) operator for the type Task, per [1]
    ///
    /// Signature:
    /// m a -> (a -> m b) -> m b
    /// where:
    ///   m: Task
    ///   a: T
    ///   b: U
    ///   (a -> m b): f
    /// 
    /// [1] https://www.haskell.org/tutorial/monads.html
    /// </summary>
    public static Task<U> Select<T, U>(
        this Task<T> targetTask,
        Func<T, Task<U>> f) => Apply(targetTask, f);

    private static async Task<U> Apply<T, U>(Task<T> targetTask, Func<T, Task<U>> f)
        => await f(await targetTask);

    /// <summary>
    /// Like the monadic >>= (bind) operator .Select(), but the type of f is
    /// a -> b
    /// instead of
    /// a -> m b
    /// </summary>
    public static Task<U> Select<T, U>(
        this Task<T> targetTask,
        Func<T, U> f) => Apply(targetTask, f);

    private static async Task<U> Apply<T, U>(Task<T> targetTask, Func<T, U> f)
        => f(await targetTask);
}