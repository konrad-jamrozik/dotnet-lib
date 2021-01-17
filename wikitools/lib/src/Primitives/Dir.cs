using System.IO;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Primitives
{
    public class Dir
    {
        private IFileSystem FileSystem { get; }

        public Dir(IFileSystem fs, string path)
        {
            FileSystem = fs;
            Value = path;
        }

        public string Value { get; }

        public bool Exists() => FileSystem.DirectoryExists(Value);
    }
}
