using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnRequestsAndCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CancelledByUser",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledByUserAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReturnRequests",
                columns: table => new
                {
                    ReturnRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DetailedReason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequests", x => x.ReturnRequestId);
                    table.ForeignKey(
                        name: "FK_ReturnRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestLines",
                columns: table => new
                {
                    ReturnRequestLineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnRequestId = table.Column<int>(type: "integer", nullable: false),
                    CartLineId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestLines", x => x.ReturnRequestLineId);
                    table.ForeignKey(
                        name: "FK_ReturnRequestLines_CartLine_CartLineId",
                        column: x => x.CartLineId,
                        principalTable: "CartLine",
                        principalColumn: "CartLineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnRequestLines_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequests",
                        principalColumn: "ReturnRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestLines_CartLineId",
                table: "ReturnRequestLines",
                column: "CartLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestLines_ReturnRequestId",
                table: "ReturnRequestLines",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_OrderId",
                table: "ReturnRequests",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnRequestLines");

            migrationBuilder.DropTable(
                name: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledByUser",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledByUserAt",
                table: "Orders");
        }
    }
}
