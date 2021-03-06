using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    // kja try to make it nonpublic
    public record TreeNode<TValue>(TValue Value, IList<TreeNode<TValue>> Children)
    {
    }
}