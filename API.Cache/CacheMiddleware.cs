namespace API.Cache;

using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using API.Cache.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

public class CacheMiddleware
{
    private static readonly string[] _varyingContextHeaders = { HeaderNames.Accept, HeaderNames.AcceptLanguage }; // NOTE: Potentially these should be configurable from the outside.
    private const int MaxCachedContent = 20 * 1024 * 1024;

    private readonly RequestDelegate _next;
    private readonly ICacheStore _cacheStore;

    public CacheMiddleware(RequestDelegate next, ICacheStore cacheStore) // A middleware is constructed once for the lifetime of an application. The cache store in general should be a singleton so this should not be a problem.
    {
        this._next = next ?? throw new ArgumentNullException(nameof(next));
        this._cacheStore = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
    }

    public async Task Invoke(HttpContext httpContent)
    {
        var request = httpContent.Request;
        var isModificationRequest = IsModificationRequest(request);
        
        var response = httpContent.Response;

        var cacheKey = GenerateCacheKey(request);
        var cacheRecord = await this._cacheStore.GetAsync(cacheKey, httpContent.RequestAborted);

        var skipRequestExecution = false;
        if (cacheRecord?.ETag is not null)
        {
            if (!isModificationRequest)
            {
                skipRequestExecution = request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNonMatch) && ETagEquals(cacheRecord.ETag, ifNonMatch.ToString(), useStrongComparison: false);

                if (skipRequestExecution) // We have a cache hit.
                {
                    response.StatusCode = StatusCodes.Status304NotModified;
                    SetETag(response, cacheRecord.ETag);
                }
            }
            else
            {
                skipRequestExecution = request.Headers.TryGetValue(HeaderNames.IfMatch, out var ifMatch) && ETagEquals(cacheRecord.ETag, ifMatch.ToString(), useStrongComparison: true) == false;

                if (skipRequestExecution) // We have a failed pre-condition
                {
                    response.StatusCode = StatusCodes.Status412PreconditionFailed;
                    SetETag(response, cacheRecord.ETag);
                }
            }
        }

        if (skipRequestExecution == false)
        {
            var cacheRecordFromResponse = await this.HandleResponse(httpContent);
            
            // NOTE: It would be great if our cache store provides us with transactions so we can avoid potential race conditions. 
            if (isModificationRequest && ResponseIsSuccessful(response)) await this._cacheStore.InvalidateAsync(GetRequestPath(request), httpContent.RequestAborted);
            if (cacheRecordFromResponse is not null) await this._cacheStore.SetAsync(cacheKey, cacheRecordFromResponse, httpContent.RequestAborted);
        }
    }

    private async Task<CacheRecord> HandleResponse(HttpContext httpContext)
    {
        var response = httpContext.Response;
        var responseBodyStream = response.Body;

        using var buffer = new MemoryStream();
        response.Body = buffer;

        CacheRecord generatedRecord = null;
        try
        {
            await this._next.Invoke(httpContext);

            if (ResponseIsSuccessful(response))
            {
                ETag eTag = null;
                var contentLength = buffer.Length;
                if (contentLength <= MaxCachedContent)
                    eTag = await GenerateETagAsync(buffer, httpContext.RequestAborted);

                if (eTag is not null)
                {
                    generatedRecord = new CacheRecord(eTag);
                    SetETag(response, eTag);
                }
            }

            buffer.Seek(0, SeekOrigin.Begin);
            if (buffer.Length > 0) await buffer.CopyToAsync(responseBodyStream); // After this line we can no longer set headers - the response has started.
        }
        finally
        {
            httpContext.Response.Body = responseBodyStream;
        }

        return generatedRecord;
    }

    private static bool IsModificationRequest(HttpRequest request) => request.Method == HttpMethods.Put || request.Method == HttpMethods.Delete; // You can think about post/patch methods

    private static CacheKey GenerateCacheKey(HttpRequest request)
    {
        var varyingHeaderValues = _varyingContextHeaders.Select(vch => request.Headers.TryGetValue(vch, out var val) ? val.ToString() : string.Empty);
        var varyingContext = string.Join('-', varyingHeaderValues);
        // NOTE: With the following implementation we can include all kind of contextual information here, e.g. user-specific data (identifier, language preferences, culture preferences, etc.)
        // However, the order of these contextual data segments should be preserved (and empty values should be included as well) in ordered to avoid having unstructured data as it may cause troubles.

        return new CacheKey(GetRequestPath(request), GetQueryString(request), varyingContext);
    }
    
    private static string GetRequestPath(HttpRequest request) => request.Path.HasValue ? request.Path.Value.ToLowerInvariant() : string.Empty;
    
    private static string GetQueryString(HttpRequest request) => request.QueryString.HasValue ? request.QueryString.Value.ToLowerInvariant() : string.Empty;

    private static bool ResponseIsSuccessful(HttpResponse response) => response.StatusCode >= 200 && response.StatusCode < 300;

    private static bool ETagEquals(ETag originalETag, string value, bool useStrongComparison)
    {
        if (originalETag is null || string.IsNullOrWhiteSpace(value)) return false;

        var parsedETag = ETag.Parse(value);
        if (useStrongComparison && (!originalETag.IsStrong || !parsedETag.IsStrong)) return false;
        return string.Equals(originalETag.Value, parsedETag.Value, StringComparison.OrdinalIgnoreCase);
    }

    private static void SetETag(HttpResponse response, ETag eTag)
    {
        if (response is null || eTag is null) return;

        response.Headers[HeaderNames.ETag] = eTag.ToString();
    }

    // The following method will be useful if you ever need to use `LastModified` in cache validation.
    /*
    private static bool TryParseDateTimeOffset(string value, out DateTimeOffset dateTimeOffset)
    {
        dateTimeOffset = default;
        if (!string.IsNullOrWhiteSpace(value) && DateTimeOffset.TryParseExact(value, "R", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out var parsedDateTimeOffset))
            dateTimeOffset = parsedDateTimeOffset;

        return dateTimeOffset != default;
    }
    */

    private static async Task<ETag> GenerateETagAsync(Stream stream, CancellationToken cancellationToken)
    {
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