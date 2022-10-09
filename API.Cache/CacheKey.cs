namespace API.Cache;

public record CacheKey(string RequestPath, string QueryString, string VaryingContext);