namespace Utilities;

public static class CollectionExtensions
{
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> collection)
        => collection ?? Enumerable.Empty<T>();

    public static IEnumerable<T> IgnoreNullValues<T>(this IEnumerable<T> collection)
        where T : class
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return collection.Where(el => el is not null);
    }

    public static IEnumerable<T> IgnoreDefaultValues<T>(this IEnumerable<T> collection)
        where T : struct, IEquatable<T>
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return collection.Where(el => !el.Equals(default));
    }

    public static IEnumerable<string> IgnoreNullOrWhitespaceValues(this IEnumerable<string> collection)
    {
        if (collection is null) 
            throw new ArgumentNullException(nameof(collection));

        return collection.Where(el => string.IsNullOrWhiteSpace(el) == false);
    }

    public static IEnumerable<T> AsEnumerable<T>(this T value) => new [] { value };

    public static IEnumerable<T> ConcatenateWith<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        foreach (var element in a.OrEmptyIfNull()) yield return element;
        foreach (var element in b.OrEmptyIfNull()) yield return element;
    }

    public static IEnumerable<T> ConcatenateWith<T>(this IEnumerable<T> a, T b)
    {
        foreach (var element in a.OrEmptyIfNull()) yield return element;
        yield return b;
    }
}