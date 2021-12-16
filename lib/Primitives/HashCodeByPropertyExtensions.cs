using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wikitools.Lib.Primitives;

public static class HashCodeByPropertyExtensions
{
    // https://stackoverflow.com/questions/23468671/what-is-the-best-way-to-implement-gethashcode-for-class-with-lots-of-propertie
    // https://stackoverflow.com/questions/28546239/handling-collections-in-gethashcode-implementation
    // - https://stackoverflow.com/a/28548880/986533
    // https://stackoverflow.com/questions/7757365/does-linq-work-with-ienumerable
    public static int GetHashCodeOnProperties<T>(this T target)
    {
        var props  = target!.GetType().GetProperties().Where(prop => prop.MemberType == MemberTypes.Property);
        var values = props.Select(prop => prop.GetValue(target)).Where(val => val != null);
        return values.GetEnumerableHashCode();
    }

    private static int GetEnumerableHashCode<T>(this IEnumerable<T> enumerable) =>
        enumerable
            .Where(item => item switch
            {
                IEnumerable<object?> enumerableItem => enumerableItem.Any(),
                _ => item != null
            })
            .Select(item => item switch
            {
                IEnumerable<object?> enumerableItem => enumerableItem.GetEnumerableHashCode(),
                _ => item!.GetHashCode()
            })
            .Aggregate((total, nextCode) => (total * 397) ^ nextCode);
}