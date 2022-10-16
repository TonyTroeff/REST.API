namespace API.ContentNegotiation.Contracts;

using System.Diagnostics.CodeAnalysis;

public interface IContentFormatManager<TEntity>
{
    ContentFormatDescriptor<TEntity> GetContentFormat(string mediaType);
    
    [return: NotNull]
    ContentFormatDescriptor<TEntity> GetDefaultContentFormat();
}