namespace Data;

using Data.Contracts;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SeminarDbContext : DbContext
{
    public SeminarDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shop>(
            shopEntityBuilder =>
            {
                shopEntityBuilder.ToTable("Shops");
                UseDefaultConfiguration(shopEntityBuilder);
                shopEntityBuilder.Property(s => s.Name).IsRequired().IsUnicode().HasColumnName("n");
                shopEntityBuilder.Property(s => s.Address).IsRequired().IsUnicode().HasColumnName("a");
            });
    }

    private static void UseDefaultConfiguration<TEntity>(EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class, IEntity
    {
        if (entityTypeBuilder is null) throw new ArgumentNullException(nameof(entityTypeBuilder));
        entityTypeBuilder.HasKey(x => x.Id);
        entityTypeBuilder.Property(x => x.Created).IsRequired().HasColumnName("c");
        entityTypeBuilder.Property(x => x.LastModified).IsRequired().HasColumnName("lm");
    }
}