namespace API.Cache;

public record CacheRecord(ETag ETag); // NOTE: You can include `LastModified` here if you want to support this.