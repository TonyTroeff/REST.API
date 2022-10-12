namespace API.Cache;

using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using API.Cache.Contracts;

public class DefaultETagGenerator : IETagGenerator
{
    public async Task<ETag> GenerateAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream is null || !stream.CanSeek) return null;
        
        var buffer = ArrayPool<byte>.Shared.Rent(1024);

        string hash;
        try
        {
            using var md5 = MD5.Create();
            var totalReadBytes = 0;

            stream.Seek(0, SeekOrigin.Begin);
            while (totalReadBytes < stream.Length)
            {
                var currentReadBytes = await stream.ReadAsync(buffer, cancellationToken);
                totalReadBytes += currentReadBytes;

                md5.TransformBlock(buffer, 0, currentReadBytes, null, 0);
            }

            md5.TransformFinalBlock(buffer, 0, 0);

            var hashBytes = md5.Hash;
            if (hashBytes is null) hash = string.Empty;
            else
            {
                var sb = new StringBuilder(capacity: hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append($"{b:X2}");

                hash = sb.ToString();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        if (string.IsNullOrWhiteSpace(hash)) return null;
        return new ETag(hash, true);
    }
}