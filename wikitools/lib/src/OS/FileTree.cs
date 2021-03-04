using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    public class FileTree
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _path;

        public FileTree(IFileSystem fileSystem, string path)
        {
            _fileSystem = fileSystem;
            _path = path;
            
        }

        // kj2 consider making FileTree implement TreeData<string> instead
        // This will possibly require making th record abstract, and also
        // the problem of task / laziness needs to be addressed
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        public Task<TreeData<string>> TreeData()
        {
            // kja implement properly walking the tree, with decoupled IFilesystem etc.
            var directoryInfo = new DirectoryInfo(_path);
            var fileInfos     = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            var paths         = fileInfos.Select(fi => Path.GetRelativePath(_path, fi.FullName));
            return Task.FromResult(new TreeData<string>(paths.ToArray()));
        }
    }
}