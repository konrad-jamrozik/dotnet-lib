using System;
using Xunit;

namespace Wikitools.Tests
{
    internal static class Verification
    {
        public static TReturn? Verify<TData, TReturn>(Func<TData, TReturn> target, TData data, Type? excType) 
            where TReturn : class?
        {
            TReturn? ret = null;
            try
            {
                ret = target(data);
            }
            catch (Exception e)
            {
                if (excType != null && excType.IsInstanceOfType(e))
                {
                    return ret;
                }
            }

            if (excType != null)
                Assert.False(true);

            return ret;
        }

        public static TReturn? VerifyStruct<TData, TReturn>(Func<TData, TReturn> target, TData data, Type? excType) 
            where TReturn : struct
        {
            TReturn? ret = null;
            try
            {
                ret = target(data);
            }
            catch (Exception e)
            {
                if (excType != null && excType.IsInstanceOfType(e))
                {
                    return ret;
                }
            }

            if (excType != null)
                Assert.False(true);

            return ret;
        }
    }
}