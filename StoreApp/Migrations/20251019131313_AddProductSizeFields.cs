using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSizeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresSize",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SizeOptionsCsv",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "CartLine",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 7,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 8,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 9,
                columns: new[] { "RequiresSize", "SizeOptionsCsv" },
                values: new object[] { false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresSize",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SizeOptionsCsv",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "CartLine");
        }
    }
}
