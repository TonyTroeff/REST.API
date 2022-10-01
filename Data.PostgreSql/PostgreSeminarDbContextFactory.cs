namespace Data.PostgreSql;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class PostgreSeminarDbContextFactory: IDesignTimeDbContextFactory<PostgreSeminarDbContext>
{
    public PostgreSeminarDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSeminarDbContext>();
        if (args is null || !args.Any() || string.IsNullOrWhiteSpace(args[0]))
            throw new InvalidOperationException("Please provide a valid connection string.");

        optionsBuilder.UseNpgsql(args[0]);
        return new PostgreSeminarDbContext(optionsBuilder.Options);
    }
}