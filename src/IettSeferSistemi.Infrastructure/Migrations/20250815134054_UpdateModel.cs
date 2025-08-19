using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IettSeferSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kredi",
                table: "Kullanicilar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Kredi",
                table: "Kullanicilar",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
