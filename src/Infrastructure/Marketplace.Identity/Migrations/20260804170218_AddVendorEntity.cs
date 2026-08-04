using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marketplace.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShopNamePrs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShopNamePs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionPrs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionPs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "VendorId",
                value: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "VendorId",
                value: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "BackupCodesJson", "CreatedAt", "Email", "EmailVerificationToken", "EmailVerificationTokenExpiresAt", "FullName", "IsActive", "IsEmailConfirmed", "IsLockoutEnabled", "IsTwoFactorEnabled", "LockoutEnd", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PhoneNumber", "TwoFactorSecret", "UpdatedAt" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), 0, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@noorzai.com", null, null, "System Admin", true, true, true, false, null, "dummy-hash", null, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "Id", "BannerUrl", "CommissionRate", "CreatedAt", "DescriptionEn", "DescriptionPrs", "DescriptionPs", "IsActive", "IsVerified", "LogoUrl", "Rating", "ShopNameEn", "ShopNamePrs", "ShopNamePs", "UserId" },
                values: new object[] { new Guid("66666666-6666-6666-6666-666666666666"), "", 0.10m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Official products from Noorzai.", "محصولات رسمی از طرف بازار نورزی.", "د نورزی بازار رسمي محصولات.", true, true, "", 5.0, "Noorzai Official", "فروشگاه رسمی نورزی", "د نورزی رسمي پلورنځی", new Guid("55555555-5555-5555-5555-555555555555") });

            migrationBuilder.CreateIndex(
                name: "IX_Products_VendorId",
                table: "Products",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VendorId",
                table: "OrderItems",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_UserId",
                table: "Vendors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Vendors_VendorId",
                table: "OrderItems",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Vendors_VendorId",
                table: "Products",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Vendors_VendorId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Vendors_VendorId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Products_VendorId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_VendorId",
                table: "OrderItems");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "OrderItems");
        }
    }
}
