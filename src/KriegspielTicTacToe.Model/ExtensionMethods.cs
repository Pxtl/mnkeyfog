namespace KriegspielTicTacToe.Model;

/// <summary>
/// Variant of MaxBy that returns NULL if there is not a single clear maximum.
/// </summary>
public static class ExtensionMethods {
    /// <summary>
    /// Variant of MaxBy that returns NULL if there is not a single clear maximum.
    /// </summary>
    public static Nullable<TItem> MaxByStrict<TItem, TProperty>(this ICollection<TItem> items, Func<TItem, TProperty> getter) where TItem : struct {
        var maxItem = items.MaxBy(getter);
        var maxPropVal = getter(maxItem);
        var isFound = false;
        if(items.Count == 0) {
            return null;
        }
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
}
