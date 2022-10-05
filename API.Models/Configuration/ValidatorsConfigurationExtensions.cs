namespace API.Models.Configuration;

using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class ValidatorsConfigurationExtensions
{
    public static void ConfigureApiModelValidators(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

        serviceCollection.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}