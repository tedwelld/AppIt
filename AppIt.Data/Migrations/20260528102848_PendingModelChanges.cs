using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId1",
                table: "Payments",
                column: "InvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId1",
                table: "Payments",
                column: "InvoiceId1",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId1",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_InvoiceId1",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "Payments");
        }
    }
}
