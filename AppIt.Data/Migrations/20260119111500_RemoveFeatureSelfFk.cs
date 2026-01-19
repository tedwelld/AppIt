using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    public partial class RemoveFeatureSelfFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Features_Features_FeatureIdId",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Features_FeatureIdId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "FeatureIdId",
                table: "Features");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FeatureIdId",
                table: "Features",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_FeatureIdId",
                table: "Features",
                column: "FeatureIdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Features_Features_FeatureIdId",
                table: "Features",
                column: "FeatureIdId",
                principalTable: "Features",
                principalColumn: "Id");
        }
    }
}

