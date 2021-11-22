using System.IO;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Tests
{
    public record TestFile(IFileSystem FS, string Path)
    {
        public string[] Write(IWritableToText target)
        {
            using var fileWriter = FS.CreateText(Path);
            target.WriteAsync(fileWriter);
            return FS.ReadAllLines(Path);
        }
    }
}