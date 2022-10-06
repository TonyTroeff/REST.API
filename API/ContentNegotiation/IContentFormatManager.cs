namespace API.ContentNegotiation;

public interface IContentFormatManager<TEntity>
{
    ContentFormatDescriptor GetContentFormat(string mediaType);
}