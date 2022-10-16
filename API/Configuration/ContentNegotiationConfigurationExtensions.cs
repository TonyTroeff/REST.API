namespace API.Configuration;

using API.ContentNegotiation;
using API.ContentNegotiation.Contracts;
using API.ContentNegotiation.Impl;
using Data.Models;

public static class ContentNegotiationConfigurationExtensions
{
    public static void ConfigureContentNegotiation(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

        serviceCollection.AddSingleton<IContentFormatManager<Shop>, ShopContentFormatManager>();
        serviceCollection.AddSingleton<IContentFormatManager<Product>, ProductContentFormatManager>();
    }
}