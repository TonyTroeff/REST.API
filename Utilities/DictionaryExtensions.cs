namespace Utilities;

public static class DictionaryExtensions
{
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary is null) throw new ArgumentNullException(nameof(dictionary));

        if (dictionary.TryGetValue(key, out var val)) return val;
        return default;
    }
}