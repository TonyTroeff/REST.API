namespace API.Cache.Contracts;

public interface IETagGenerator
{
    Task<ETag> GenerateAsync(Stream stream, CancellationToken cancellationToken);
}