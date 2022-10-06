namespace API.ContentNegotiation;

public static class VendorMediaTypes
{
    private const string Prefix = "application/vnd.rest-api";
    public const string ShopFull = $"{Prefix}.shop-full";
    public const string ShopMinified = $"{Prefix}.shop-minified";

    public static string WithHateoas(string mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType)) return string.Empty;
        return $"{mediaType}.hateoas";
    }

    public static string WithSuffix(string mediaType, string suffix)
    {
        if (string.IsNullOrWhiteSpace(mediaType)) return string.Empty;
        if (string.IsNullOrWhiteSpace(suffix)) return mediaType;
        return $"{mediaType}+{suffix}";
    }
}