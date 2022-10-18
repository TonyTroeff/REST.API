namespace API.Cache;

using API.Cache.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

// In order to have a reliable and scalable cache infrastructure, we should have a persistent cache store (MongoDB, Redis) and a refined mechanism to define which requests would be affected by a successfully executed modification action (resulting in an optimal invalidation mechanism).
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

    public async Task Invoke(HttpContext httpContent, IETagGenerator eTagGenerator)
    {
        var request = httpContent.Request;

        var response = httpContent.Response;

        var cacheKey = GenerateCacheKey(request);
        var cacheRecord = await this._cacheStore.GetAsync(cacheKey, httpContent.RequestAborted);
        var cacheWasHit = cacheRecord?.ETag is not null;

        var skipRequestExecution = false;
        if (HttpMethods.IsPut(request.Method) || HttpMethods.IsDelete(request.Method)) // Depending on some explicit circumstances, POST calls may be included here (but this is very negotiable).
        {
            // If no cache record was found, we can assume that the cache has been previously invalidated and so it is safer to return 412 Precondition Failed and force the client to request the GET endpoint at the same route again. 
            skipRequestExecution = request.Headers.TryGetValue(HeaderNames.IfMatch, out var ifMatch) && (!cacheWasHit || ETagEquals(cacheRecord.ETag, ifMatch.ToString(), useStrongComparison: true) == false);

            if (skipRequestExecution) // We have a cache miss or the ETag is different
                response.StatusCode = StatusCodes.Status412PreconditionFailed;
        }
        else if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
        {
            skipRequestExecution = cacheWasHit && request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNonMatch) && ETagEquals(cacheRecord.ETag, ifNonMatch.ToString(), useStrongComparison: false);

            if (skipRequestExecution) // We have a cache hit and the ETag is still the same.
                response.StatusCode = StatusCodes.Status304NotModified;
        }

        if (skipRequestExecution)
        {
            if (cacheWasHit) SetETag(response, cacheRecord.ETag);
        }
        else
        {
            // We use this revision number to handle race conditions.
            // If it has already expired when we try to set the cache record at the end, nothing should be stored.
            var revisionNumber = await this._cacheStore.GetRevisionNumberAsync(cacheKey, httpContent.RequestAborted);
            
            var cacheRecordFromResponse = await this.HandleResponse(httpContent, eTagGenerator);

            var isModificationRequest = HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsDelete(request.Method);
            if (isModificationRequest && ResponseIsSuccessful(response)) revisionNumber = await this._cacheStore.InvalidateAsync(cacheKey, httpContent.RequestAborted);

            if (cacheRecordFromResponse is not null)
            {
                if (response.StatusCode == StatusCodes.Status201Created && response.Headers.TryGetValue(HeaderNames.Location, out var location))
                {
                    var charactersToSkip = request.Scheme.Length + request.Host.Value.Length + 3; // {request.Scheme}://{request.Host}
                    var createdAtLocation = location.ToString();
                    var createdAtRequestPath = createdAtLocation[charactersToSkip..];
                    cacheKey = cacheKey with { RequestPath = createdAtRequestPath };
                }

                await this._cacheStore.SetAsync(revisionNumber, cacheKey, cacheRecordFromResponse, httpContent.RequestAborted);
            }
        }
    }

    private async Task<CacheRecord> HandleResponse(HttpContext httpContext, IETagGenerator eTagGenerator)
    {
        var response = httpContext.Response;
        var responseBodyStream = response.Body;

        using var buffer = new MemoryStream();
        response.Body = buffer;

        CacheRecord generatedRecord = null;
        try
        {
            await this._next.Invoke(httpContext);

            if (buffer.Length > 0)
            {
                if (ResponseIsSuccessful(response))
                {
                    ETag eTag = null;
                    var contentLength = buffer.Length;
                    if (contentLength <= MaxCachedContent)
                        eTag = await eTagGenerator.GenerateAsync(buffer, httpContext.RequestAborted);

                    if (eTag is not null)
                    {
                        generatedRecord = new CacheRecord(eTag);
                        SetETag(response, eTag);
                    }
                }
                
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(responseBodyStream); // After this line we can no longer set headers - the response has started.
            }
        }
        finally
        {
            httpContext.Response.Body = responseBodyStream;
        }

        return generatedRecord;
    }

    private static CacheKey GenerateCacheKey(HttpRequest request)
    {
        var requestPath = request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var queryString = request.QueryString.Value?.ToLowerInvariant() ?? string.Empty;

        var varyingHeaderValues = _varyingContextHeaders.Select(vch => request.Headers.TryGetValue(vch, out var val) ? val.ToString() : string.Empty);
        var varyingContext = string.Join('-', varyingHeaderValues);
        // NOTE: With the following implementation we can include all kind of contextual information here, e.g. user-specific data (identifier, language preferences, culture preferences, etc.)
        // However, the order of these contextual data segments should be preserved (and empty values should be included as well) in ordered to avoid having unstructured data as it may cause troubles.

        return new CacheKey(requestPath, queryString, varyingContext);
    }

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
}