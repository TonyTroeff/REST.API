using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    n = table.Column<string>(type: "text", nullable: false),
                    a = table.Column<string>(type: "text", nullable: false),
                    c = table.Column<long>(type: "bigint", nullable: false),
                    lm = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
