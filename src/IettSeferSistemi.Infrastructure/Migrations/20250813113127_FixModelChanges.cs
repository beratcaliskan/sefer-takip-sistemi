using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IettSeferSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Hatlar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaslangicDuragi",
                table: "Hatlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BitisDuragi",
                table: "Hatlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurakSayisi",
                table: "Hatlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MesafeKm",
                table: "Hatlar",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TahminSureDakika",
                table: "Hatlar",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Hatlar");

            migrationBuilder.DropColumn(
                name: "BaslangicDuragi",
                table: "Hatlar");

            migrationBuilder.DropColumn(
                name: "BitisDuragi",
                table: "Hatlar");

            migrationBuilder.DropColumn(
                name: "DurakSayisi",
                table: "Hatlar");

            migrationBuilder.DropColumn(
                name: "MesafeKm",
                table: "Hatlar");

            migrationBuilder.DropColumn(
                name: "TahminSureDakika",
                table: "Hatlar");
        }
    }
}
