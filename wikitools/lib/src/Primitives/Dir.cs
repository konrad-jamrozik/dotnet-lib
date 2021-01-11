using System.IO;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Primitives
{
    public class Dir
    {
        private readonly LazyString _path;
        public IFileSystem FileSystem { get; }

        private Dir(IFileSystem fs, LazyString path)
        {
            FileSystem = fs;
            _path = path;
        }

        public Dir(IFileSystem fs, string path) : this(fs, (LazyString) path) {}

        public Dir(Dir parent, string child) : this
        (
            parent.FileSystem, 
            new LazyString(() => parent.Value + Path.DirectorySeparatorChar + child)
        ) 
        {}

        public string Value => _path.Value;

        public bool Exists() => FileSystem.DirectoryExists(Value);
    }
}
