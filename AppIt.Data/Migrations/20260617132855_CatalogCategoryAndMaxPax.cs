using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogCategoryAndMaxPax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPax",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPax",
                table: "Tours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Tours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPax",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPax",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId",
                table: "Accommodations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ProductCategoryId",
                table: "Transfers",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_ProductCategoryId",
                table: "Tours",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ProductCategoryId",
                table: "Activities",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_ProductCategoryId",
                table: "Accommodations",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accommodations_ProductCategories_ProductCategoryId",
                table: "Accommodations",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_ProductCategories_ProductCategoryId",
                table: "Activities",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tours_ProductCategories_ProductCategoryId",
                table: "Tours",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_ProductCategories_ProductCategoryId",
                table: "Transfers",
                column: "ProductCategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accommodations_ProductCategories_ProductCategoryId",
                table: "Accommodations");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_ProductCategories_ProductCategoryId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Tours_ProductCategories_ProductCategoryId",
                table: "Tours");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_ProductCategories_ProductCategoryId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_ProductCategoryId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Tours_ProductCategoryId",
                table: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ProductCategoryId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Accommodations_ProductCategoryId",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "MaxPax",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "MaxPax",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "MaxPax",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaxPax",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Accommodations");
        }
    }
}
