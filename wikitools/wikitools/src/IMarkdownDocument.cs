using System.Collections.Generic;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public interface IMarkdownDocument : IWritableToText
    {
        List<object> Content { get; }
    }
}