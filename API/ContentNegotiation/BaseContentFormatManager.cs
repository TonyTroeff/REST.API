namespace API.ContentNegotiation;

using Microsoft.Net.Http.Headers;

public abstract class BaseContentFormatManager<TEntity> : IContentFormatManager<TEntity>
{
    private readonly Dictionary<string, VendorConfig> _supportedVendors = new ();
    private string _defaultVendor;
    
    public ContentFormatDescriptor<TEntity> GetContentFormat(string mediaType)
    {
        this.EnsureRegisteredVendors();
        if (string.IsNullOrWhiteSpace(mediaType) || MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType) == false) return null;

        var mediaTypeId = $"{parsedMediaType.Type}/{parsedMediaType.SubTypeWithoutSuffix}";
        if (this._supportedVendors.TryGetValue(mediaTypeId, out var vendorConfig) == false || vendorConfig is null) return null;

        var hateoasParameter = NameValueHeaderValue.Find(parsedMediaType.Parameters, "hateoas");
        var addHateoasLinks = hateoasParameter is not null && hateoasParameter.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
        return new ContentFormatDescriptor<TEntity>(vendorConfig.OutputType, addHateoasLinks, vendorConfig.Transforms);
    }

    public ContentFormatDescriptor<TEntity> GetDefaultContentFormat()
    {
        this.EnsureRegisteredVendors();
        return Instantiate(this._supportedVendors[this._defaultVendor], withHateoasLinks: true);
    }

    /// <remarks>The first vendor to add will be the default one.</remarks>
    protected void AddVendor(string mediaType, Type outputType, params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] transforms)
    {
        if (string.IsNullOrWhiteSpace(mediaType)) throw new ArgumentNullException(nameof(mediaType));
        if (outputType is null) throw new ArgumentNullException(nameof(outputType));

        if (this._supportedVendors.Count == 0) this._defaultVendor = mediaType;
        this._supportedVendors[mediaType] = new VendorConfig(outputType, transforms);
    }

    private static ContentFormatDescriptor<TEntity> Instantiate(VendorConfig vendorConfig, bool withHateoasLinks)
    {
        if (vendorConfig is null) throw new ArgumentNullException(nameof(vendorConfig));
        return new ContentFormatDescriptor<TEntity>(vendorConfig.OutputType, withHateoasLinks, vendorConfig.Transforms);
    }

    private void EnsureRegisteredVendors()
    {
        if (this._supportedVendors.Count == 0) throw new InvalidOperationException("No supported vendors are registered.");
    }

    private record VendorConfig(Type OutputType, Func<IQueryable<TEntity>, IQueryable<TEntity>>[] Transforms);
}