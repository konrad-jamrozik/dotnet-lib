using System.IO;
using System.Threading.Tasks;

namespace Wikitools.Lib.Primitives;

public interface IWritableToText
{
    Task WriteAsync(TextWriter textWriter);
}