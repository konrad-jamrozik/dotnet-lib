﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS
{
    public record FileTree(IFileSystem FileSystem, string Path)
    {
        // kj2 consider making FileTree implement TreeData<string> instead
        // This will possibly require making the record abstract, and also
        // the problem of task / laziness needs to be addressed
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        public Task<FilePathTrie> FilePathTrie()
        {
            // kj2 make this method Lazy
            // kj2 implement properly walking the tree: decoupled from IFilesystem
            var directoryInfo = new DirectoryInfo(Path);
            var fileInfos     = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            var paths         = fileInfos.Select(fi => System.IO.Path.GetRelativePath(Path, fi.FullName));
            return Task.FromResult(new FilePathTrie(paths.ToArray()));
        }
    }
}