// <auto-generated />
using System;
using Data.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.PostgreSql.Migrations
{
    [DbContext(typeof(PostgreSeminarDbContext))]
    [Migration("20221015081940_AddProducts")]
    partial class AddProducts
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Data.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("Created")
                        .HasColumnType("bigint")
                        .HasColumnName("c");

                    b.Property<string>("Description")
                        .IsUnicode(true)
                        .HasColumnType("text")
                        .HasColumnName("ds");

                    b.Property<string>("Distributor")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("text")
                        .HasColumnName("dt");

                    b.Property<long>("LastModified")
                        .HasColumnType("bigint")
                        .HasColumnName("lm");

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("text")
                        .HasColumnName("n");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric")
                        .HasColumnName("p");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid")
                        .HasColumnName("shid");

                    b.HasKey("Id");

                    b.HasIndex("ShopId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Data.Models.Shop", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("text")
                        .HasColumnName("a");

                    b.Property<long>("Created")
                        .HasColumnType("bigint")
                        .HasColumnName("c");

                    b.Property<long>("LastModified")
                        .HasColumnType("bigint")
                        .HasColumnName("lm");

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("text")
                        .HasColumnName("n");

                    b.HasKey("Id");

                    b.ToTable("Shops", (string)null);
                });

            modelBuilder.Entity("Data.Models.Product", b =>
                {
                    b.HasOne("Data.Models.Shop", "Shop")
                        .WithMany("Products")
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("Data.Models.Shop", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
