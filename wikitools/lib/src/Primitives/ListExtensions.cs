﻿using System.Collections.Generic;

namespace Wikitools.Lib.Primitives
{
    public static class ListExtensions
    {
        public static IList<TValue> AsList<TValue>(this TValue source) => 
            new List<TValue> { source };
    }
}