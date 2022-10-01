namespace Core.Configuration;

using Core.Contracts.Services;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Data;
using Data.Contracts;

public static class ServicesConfigurationExtensions
{
    public static void ConfigureServices(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

        serviceCollection.AddScoped(typeof(IService<>), typeof(Service<>));
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }
}