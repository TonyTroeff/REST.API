namespace API.ContentNegotiation;

using Microsoft.Net.Http.Headers;

public abstract class BaseContentFormatManager<TEntity> : IContentFormatManager<TEntity>
{
    public ContentFormatDescriptor GetContentFormat(string mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType) || MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType) == false) return null;

        var mediaTypeId = $"{parsedMediaType.Type}/{parsedMediaType.SubTypeWithoutSuffix}";
        if (this.SupportedVendors is null || !this.SupportedVendors.TryGetValue(mediaTypeId, out var vendorOutputType)) return null;

        var hateoasParameter = NameValueHeaderValue.Find(parsedMediaType.Parameters, "hateoas");
        var addHateoasLinks = hateoasParameter is not null && hateoasParameter.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
        return new ContentFormatDescriptor(vendorOutputType, addHateoasLinks);
    }

    public ContentFormatDescriptor GetDefaultContentFormat() => this.Default;

    protected abstract ContentFormatDescriptor Default { get; }
    protected abstract IDictionary<string, Type> SupportedVendors { get; }
}