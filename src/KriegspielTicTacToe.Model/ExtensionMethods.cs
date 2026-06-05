namespace KriegspielTicTacToe.Model;

/// <summary>
/// Variant of MaxBy that returns NULL if there is not a single clear maximum.
/// </summary>
public static class ExtensionMethods {
    /// <summary>
    /// Variant of MaxBy that returns NULL if there is not a single clear maximum.
    /// </summary>
    public static TItem? MaxByStrict<TItem, TProperty>(this IEnumerable<TItem> items, Func<TItem, TProperty> getter) where TItem : struct {
        var maxItem = items.MaxBy(getter);
        var maxPropVal = getter(maxItem);
        var isFound = false;
        foreach (var item in items) {
            var itemPropVal = getter(item);
            if (Object.Equals(itemPropVal, maxPropVal)) {
                if (isFound) {
                    return null; //there are 2 or more items with the maxPropVal, max is not distinct.
                } else {
                    isFound = true;
                }
            }
        }
        return maxItem;
    }

    public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey> comparer) where TKey : notnull
    {
        var pairs = source.Select(x => KeyValuePair.Create(keySelector(x), elementSelector(x)));
        return new OrderedDictionary<TKey, TElement>(pairs, comparer);
    }

    public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        var pairs = source.Select(x => KeyValuePair.Create(keySelector(x), elementSelector(x)));
        return new OrderedDictionary<TKey, TElement>(pairs);
    }
}
