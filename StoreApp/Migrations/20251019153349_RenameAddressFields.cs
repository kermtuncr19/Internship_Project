using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Line3",
                table: "Orders",
                newName: "Neighborhood");

            migrationBuilder.RenameColumn(
                name: "Line2",
                table: "Orders",
                newName: "District");

            migrationBuilder.RenameColumn(
                name: "Line1",
                table: "Orders",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Neighborhood",
                table: "Orders",
                newName: "Line3");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Orders",
                newName: "Line2");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Orders",
                newName: "Line1");
        }
    }
}
