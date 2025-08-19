using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IettSeferSistemi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertSeferDurumToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "VarisZamani",
                table: "Seferler",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // First, add a temporary column for the new enum values
            migrationBuilder.AddColumn<int>(
                name: "DurumTemp",
                table: "Seferler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update the temporary column with enum values based on string values
            migrationBuilder.Sql(@"
                UPDATE Seferler 
                SET DurumTemp = CASE 
                    WHEN Durum = 'Planlandi' THEN 0
                    WHEN Durum = 'Devam' THEN 1
                    WHEN Durum = 'Tamamlandi' THEN 2
                    WHEN Durum = 'Iptal' THEN 3
                    ELSE 0
                END
            ");

            // Drop the old string column
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Seferler");

            // Rename the temporary column to the original name
            migrationBuilder.RenameColumn(
                name: "DurumTemp",
                table: "Seferler",
                newName: "Durum");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "VarisZamani",
                table: "Seferler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // Add a temporary string column
            migrationBuilder.AddColumn<string>(
                name: "DurumTemp",
                table: "Seferler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Planlandi");

            // Convert enum values back to strings
            migrationBuilder.Sql(@"
                UPDATE Seferler 
                SET DurumTemp = CASE 
                    WHEN Durum = 0 THEN 'Planlandi'
                    WHEN Durum = 1 THEN 'Devam'
                    WHEN Durum = 2 THEN 'Tamamlandi'
                    WHEN Durum = 3 THEN 'Iptal'
                    ELSE 'Planlandi'
                END
            ");

            // Drop the int column
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Seferler");

            // Rename the temporary column back
            migrationBuilder.RenameColumn(
                name: "DurumTemp",
                table: "Seferler",
                newName: "Durum");
        }
    }
}
