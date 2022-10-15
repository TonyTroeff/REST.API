namespace API.ContentNegotiation;

using Utilities;

public record ContentFormatDescriptor<TEntity>
{
    public ContentFormatDescriptor(Type outputType, bool withHateoasLinks, IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> transforms = null)
    {
        this.OutputType = outputType;
        this.WithHateoasLinks = withHateoasLinks;
        this.Transforms = transforms.OrEmptyIfNull().IgnoreNullValues().ToList().AsReadOnly();
    }

    public Type OutputType { get; }
    public bool WithHateoasLinks { get; }
    public IReadOnlyCollection<Func<IQueryable<TEntity>, IQueryable<TEntity>>> Transforms { get; }
}