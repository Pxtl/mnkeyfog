namespace MnkeyFog.Model;

/// <summary>
/// Variant of MaxBy that returns mulitple if there is not a single clear maximum.
/// </summary>
public static class ExtensionMethods {
    /// <summary>
    /// Variant of MaxBy that returns mulitple if there is not a single clear maximum.
    /// </summary>
    public static IEnumerable<TItem> AllMaxBy<TItem, TProperty>(this IEnumerable<TItem> items, Func<TItem, TProperty> getter) where TItem : struct {
        var maxItem = items.MaxBy(getter);
        var maxPropVal = getter(maxItem);
        foreach (var item in items) {
            var itemPropVal = getter(item);
            if (Equals(itemPropVal, maxPropVal)) {
                yield return item;
            }
        }
    }

    /// <summary>
    /// We use sbyte as a type-safety hack for row/column/board indices.
    /// Arithmetic on sbyte requires casting to int, which eliminates the
    /// type-safety.  Resurrect a bit of that type-safety by adding simple
    /// increment/decrement operators.
    /// </summary>
    public static sbyte Plus1(this sbyte value)
        => ++value;

    /// <summary>
    /// We use sbyte as a type-safety hack for row/column/board indices.
    /// Arithmetic on sbyte requires casting to int, which eliminates the
    /// type-safety.  Resurrect a bit of that type-safety by adding simple
    /// increment/decrement operators.
    /// </summary>
    public static sbyte Minus1(this sbyte value)
        => --value;
}
