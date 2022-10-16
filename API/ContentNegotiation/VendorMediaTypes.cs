namespace API.ContentNegotiation;

public static class VendorMediaTypes
{
    private const string Prefix = "application/vnd.rest-api";
    public const string ShopFull = $"{Prefix}.shop-full";
    public const string ShopMinified = $"{Prefix}.shop-minified";
    public const string ProductFull = $"{Prefix}.product-full";
    public const string ProductMinified = $"{Prefix}.product-minified";

    public static IReadOnlyCollection<string> SupportedSuffixes { get; } = new List<string>(capacity: 1) { "json" }.AsReadOnly();

    public static string WithSuffix(string mediaType, string suffix)
    {
        if (string.IsNullOrWhiteSpace(mediaType)) return string.Empty;
        if (string.IsNullOrWhiteSpace(suffix)) return mediaType;
        return $"{mediaType}+{suffix}";
    }
}