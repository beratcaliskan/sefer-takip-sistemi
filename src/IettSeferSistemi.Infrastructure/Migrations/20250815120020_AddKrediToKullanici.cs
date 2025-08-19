using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IettSeferSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKrediToKullanici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Kredi",
                table: "Kullanicilar",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kredi",
                table: "Kullanicilar");
        }
    }
}
