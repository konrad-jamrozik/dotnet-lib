using System;
using System.Threading.Tasks;

namespace Wikitools.Lib.Primitives
{
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
        public static Task<U> M<T, U>(
            this Task<T> targetTask,
            Func<T, Task<U>> f) => Apply(targetTask, f);

        private static async Task<U> Apply<T, U>(Task<T> targetTask, Func<T, Task<U>> f)
            => await f(await targetTask);
    }
}