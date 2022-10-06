namespace API.ContentNegotiation;

using System.Diagnostics.CodeAnalysis;

public interface IContentFormatManager<TEntity>
{
    ContentFormatDescriptor GetContentFormat(string mediaType);
    
    [return: NotNull]
    ContentFormatDescriptor GetDefaultContentFormat();
}