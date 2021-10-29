using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Wikitools.Lib.Primitives
{
    // Based on https://blogs.msdn.microsoft.com/pfxteam/2011/01/15/asynclazyt/
    // See also:
    // https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Coordination/AsyncLazy.cs
    // http://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html
    // Point 3 in https://stackoverflow.com/a/13735418/986533
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory) : base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        {
        }

        // Thanks to this method, one can call:
        //   await asyncLazy 
        // instead of:
        //   await asyncLazy.Value
        // ReSharper disable once UnusedMember.Global
        public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
    }
}
