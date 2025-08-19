using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IettSeferSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDurakNotlarToYolcuSayim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Durak",
                table: "YolcuSayimlari",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notlar",
                table: "YolcuSayimlari",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durak",
                table: "YolcuSayimlari");

            migrationBuilder.DropColumn(
                name: "Notlar",
                table: "YolcuSayimlari");
        }
    }
}
