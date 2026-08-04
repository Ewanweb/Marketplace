using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Marketplace.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddECommerceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NamePrs = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NamePs = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TitlePrs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitlePs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionPrs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionPs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableSizes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableColors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "IconName", "IsActive", "NameEn", "NamePrs", "NamePs" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "local_florist", true, "Dry Fruits & Saffron", "میوه خشک و زعفران ممتاز", "وچه میوه او ممتاز زعفران" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "square_foot", true, "Handmade Afghan Carpets", "قالین‌های دستی افغانی", "د افغانستان لاسي غالۍ" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "checkroom", true, "Traditional Apparel", "پوشاک عنعنوی و مدرن", "عنعنوي او عصري جامې" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "devices", true, "Electronics & Gadgets", "لوازم الکترونیکی و دیجیتال", "الکټرونیکي او ډیجیټل توکي" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvailableColors", "AvailableSizes", "CategoryId", "CreatedAt", "DescriptionEn", "DescriptionPrs", "DescriptionPs", "ImageUrl", "IsActive", "Price", "Rating", "StockQuantity", "TitleEn", "TitlePrs", "TitlePs" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "Red", "5g,10g,25g", new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "100% pure organic super nagin saffron harvested from Herat fields.", "زعفران ۱۰۰٪ طبیعی و خالص سوپر نگین برداشت شده از مزارع ولایت هرات.", "د هرات ولایت له کروندو څخه راټول شوي ۱۰۰٪ طبعي او خالص سوپر نګین زعفران.", "https://images.unsplash.com/photo-1615485290382-441e4d049cb5?w=500", true, 45.00m, 4.9000000000000004, 100, "Herat Red Gold Premium Saffron 10g", "زعفران ممتاز طلای سرخ هرات ۱۰ گرام", "د هرات ممتاز سور زر زعفران ۱۰ ګرامه" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), "Maroon,Navy,Gold", "1.5x2m,2x3m", new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Authentic hand-knotted traditional Afghan carpet with intricate patterns.", "قالین بافته‌شده با دست با نخ‌های ابریشمی و پشم طبیعی با نقش‌های اصیل عنعنوی.", "د وریښمنو او طبعي پشم تارونو څخه په لاس اوبدل شوې د اصیلو نقشو غالۍ.", "https://images.unsplash.com/photo-1600121848594-d8644e57abab?w=500", true, 280.00m, 4.7999999999999998, 20, "Handcrafted Wool Silk Rug (1.5x2m)", "قالین دستی ابریشمی پشمی هرات (۱.۵ در ۲ متر)", "د پشم او ورېښمو لاسي غالۍ (۱.۵ په ۲ متره)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
