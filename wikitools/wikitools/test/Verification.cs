using System;
using Xunit;
using Xunit.Sdk;

namespace Wikitools.Tests
{
    // kja 3 come up with better class name
    internal static class Verification
    {
        public static TReturn? Verify<TData, TReturn>(Func<TData, TReturn> target, TData data, Type? excType) 
            where TReturn : class?
        {
            TReturn? ret;
            try
            {
                ret = target(data);
            }
            catch (Exception e)
            {
                if (excType != null && excType.IsInstanceOfType(e))
                    return null;
                throw;
            }

            if (excType != null)
                Assert.False(true, $"Expected {excType}");

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
            catch (Exception e) when (e is not XunitException)
            {
                if (excType != null && excType.IsInstanceOfType(e))
                {
                    return ret;
                }

                Assert.False(true, e.Message + Environment.NewLine + e.StackTrace);
            }

            if (excType != null)
                Assert.False(true, $"Expected exception of type {excType}");

            return ret;
        }
    }
}