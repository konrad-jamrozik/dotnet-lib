using System.Collections;
using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    public record TreeData<T>(IEnumerable<T> Data) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}