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

        modelBuilder.Entity<Product>(
            productEntityBuilder =>
            {
                productEntityBuilder.ToTable("Products");
                UseDefaultConfiguration(productEntityBuilder);
                productEntityBuilder.Property(p => p.Name).IsRequired().IsUnicode().HasColumnName("n");
                productEntityBuilder.Property(p => p.Description).IsUnicode().HasColumnName("ds");
                productEntityBuilder.Property(p => p.Distributor).IsRequired().IsUnicode().HasColumnName("dt");
                productEntityBuilder.Property(p => p.Price).IsRequired().HasColumnName("p");
                productEntityBuilder.Property(p => p.ShopId).IsRequired().HasColumnName("shid");

                productEntityBuilder.HasOne(p => p.Shop).WithMany(s => s.Products).HasForeignKey(p => p.ShopId);
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