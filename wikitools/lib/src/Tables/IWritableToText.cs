using System.IO;
using System.Threading.Tasks;

namespace Wikitools.Lib.Tables
{
    public interface IWritableToText
    {
        Task WriteAsync(TextWriter textWriter);
    }
}