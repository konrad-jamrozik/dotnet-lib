using System.Collections;
using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    public record TreeData<T>(IEnumerable<T> Data) : IEnumerable<T>
    {
        private List<(int depth, T)> BuildPreorderTree(IEnumerable<T> data)
        {
            // kja curr work
            return new List<(int depth, T)>();
        }

        public IEnumerable<(int depth, T)> AsPreorderEnumerable()
        {
            List<(int depth, T)> preorderTree = BuildPreorderTree(Data);

            // kj2 instead of building the preorderTree at once, do yield. Also:
            // - the record ctor probably should have the raw tree data as ctor input,
            // and the serialized format should be one of the "From" method.
            //   - The From method probably should take lambda for splitting the input entries,
            //   possibly with well-known case for strings.
            return preorderTree;
        }

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /* kja this record should implement generic algorithm on Tree-like data structures.
        That allows to project into nested structure.

        Namely, entries like:
        foo/bar1/baz1
        foo/bar1/baz2
        foo/bar2/baz1
        foo/bar2/baz1
        should be projectable to:
        foo
          bar1
            baz1
            baz2
          bar2
            baz3
            baz4

        It should be possible to provide the input entries as a series of leafs with separators,
        as in the example above. 
        However, internally, the record probably should keep the data efficiently, via a tree.

        */
    }
}