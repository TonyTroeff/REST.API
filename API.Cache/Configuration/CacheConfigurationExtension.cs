namespace API.Cache.Configuration;

using API.Cache.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class CacheConfigurationExtension
{
    public static void ConfigureCache(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

        serviceCollection.AddSingleton<ICacheStore, InMemoryCacheStore>();
        serviceCollection.AddSingleton<IETagGenerator, DefaultETagGenerator>();
    }

    public static void UseCache(this IApplicationBuilder applicationBuilder)
    {
        if (applicationBuilder is null) throw new ArgumentNullException(nameof(applicationBuilder));

        applicationBuilder.UseMiddleware<CacheMiddleware>();
    }
}