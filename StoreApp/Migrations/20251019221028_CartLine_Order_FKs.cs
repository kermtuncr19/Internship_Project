using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreApp.Migrations
{
    public partial class CartLine_Order_FKs : Migration
    {
        protected override void Up(MigrationBuilder m)
        {
            // 1) Eski FK'leri kaldır (zaten sizde var)
            m.DropForeignKey(name: "FK_CartLine_Orders_OrderId", table: "CartLine");
            m.DropForeignKey(name: "FK_CartLine_Products_ProductId", table: "CartLine");

            // 2) Geçmişte default 0 atanmış olabilir; önce bu satırları temizleyelim
            //    (OrderId=0 veya NULL olan satırlar FK'yi bozar)
            m.Sql(@"DELETE FROM ""CartLine""
                    WHERE ""OrderId"" IS NULL OR ""OrderId"" = 0;");

            // 3) OrderId'yi NOT NULL yap, AMA **defaultValue vermeden**!
            m.AlterColumn<int>(
                name: "OrderId",
                table: "CartLine",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // 4) FK'leri doğru davranışlarla tekrar ekle
            m.AddForeignKey(
                name: "FK_CartLine_Orders_OrderId",
                table: "CartLine",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            // Ürün FK'si: ürün silinirse satır kalsın istiyorsanız Restrict mantıklı
            m.AddForeignKey(
                name: "FK_CartLine_Products_ProductId",
                table: "CartLine",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder m)
        {
            m.DropForeignKey(name: "FK_CartLine_Orders_OrderId", table: "CartLine");
            m.DropForeignKey(name: "FK_CartLine_Products_ProductId", table: "CartLine");

            m.AlterColumn<int>(
                name: "OrderId",
                table: "CartLine",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            m.AddForeignKey(
                name: "FK_CartLine_Orders_OrderId",
                table: "CartLine",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            m.AddForeignKey(
                name: "FK_CartLine_Products_ProductId",
                table: "CartLine",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
