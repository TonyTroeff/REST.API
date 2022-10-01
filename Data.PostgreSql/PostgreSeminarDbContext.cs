namespace Data.PostgreSql;

using Microsoft.EntityFrameworkCore;

public class PostgreSeminarDbContext : SeminarDbContext
{
    public PostgreSeminarDbContext(DbContextOptions<PostgreSeminarDbContext> options)
        : base(options)
    {
    }
}