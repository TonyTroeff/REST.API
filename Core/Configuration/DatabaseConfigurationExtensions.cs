namespace Core.Configuration;

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Data.Configuration;
using Data.PostgreSql;

public static class DatabaseConfigurationExtensions
{
    public static void SetupDatabase([NotNull] this IServiceCollection services, [NotNull] DatabaseConfiguration databaseConfiguration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (databaseConfiguration is null) throw new ArgumentNullException(nameof(databaseConfiguration));

        if (string.Equals(databaseConfiguration.Provider, "postgre", StringComparison.InvariantCultureIgnoreCase)) services.SetupPostgreSql(databaseConfiguration.ConnectionString);
        else throw new InvalidOperationException("Invalid database provider.");
    }

    private static void SetupPostgreSql([NotNull] this IServiceCollection services, string connectionString)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        services.AddDbContext<DbContext, PostgreSeminarDbContext>(options => options.UseNpgsql(connectionString));
    }
}