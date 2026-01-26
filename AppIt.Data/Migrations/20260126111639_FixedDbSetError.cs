using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedDbSetError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Companies_AgentId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AgentId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "Customer");

            migrationBuilder.AddColumn<int>(
                name: "AgentCompanyId",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AgentCompanyId",
                table: "Customer",
                column: "AgentCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Companies_AgentCompanyId",
                table: "Customer",
                column: "AgentCompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Companies_AgentCompanyId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_AgentCompanyId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "AgentCompanyId",
                table: "Customer");

            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_AgentId",
                table: "Customer",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Companies_AgentId",
                table: "Customer",
                column: "AgentId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
