namespace API.ContentNegotiation;

public abstract class BaseContentFormatManager<TEntity> : IContentFormatManager<TEntity>
{
    public ContentFormatDescriptor GetContentFormat(string mediaType)
    {
        if (this.SupportedVendors is not null && this.SupportedVendors.TryGetValue(mediaType, out var vendorFormatDescriptor)) return vendorFormatDescriptor;
        return this.Default;
    }
    
    protected abstract ContentFormatDescriptor Default { get; }
    protected abstract IDictionary<string, ContentFormatDescriptor> SupportedVendors { get; }
}