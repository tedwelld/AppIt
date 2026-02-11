using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges_20260211 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Roles_RoleId1",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Companies_AgentCompanyId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Features_Permissions_PermissionId",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_RoleFeatures_RoleId",
                table: "RoleFeatures");

            migrationBuilder.DropIndex(
                name: "IX_RoleFeaturePermissions_RoleId",
                table: "RoleFeaturePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_RoleId1",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedBy = table.Column<int>(type: "int", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleFeatures_RoleId_FeatureId",
                table: "RoleFeatures",
                columns: new[] { "RoleId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleFeaturePermissions_RoleId_FeatureId_PermissionId",
                table: "RoleFeaturePermissions",
                columns: new[] { "RoleId", "FeatureId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportSnapshots_GeneratedByUserId",
                table: "ReportSnapshots",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturePermissions_FeatureId_PermissionId",
                table: "FeaturePermissions",
                columns: new[] { "FeatureId", "PermissionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Companies_AgentCompanyId",
                table: "Customers",
                column: "AgentCompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Features_Permissions_PermissionId",
                table: "Features",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Accounts_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportSnapshots_Accounts_GeneratedByUserId",
                table: "ReportSnapshots",
                column: "GeneratedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Companies_AgentCompanyId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Features_Permissions_PermissionId",
                table: "Features");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Accounts_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportSnapshots_Accounts_GeneratedByUserId",
                table: "ReportSnapshots");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_RoleFeatures_RoleId_FeatureId",
                table: "RoleFeatures");

            migrationBuilder.DropIndex(
                name: "IX_RoleFeaturePermissions_RoleId_FeatureId_PermissionId",
                table: "RoleFeaturePermissions");

            migrationBuilder.DropIndex(
                name: "IX_ReportSnapshots_GeneratedByUserId",
                table: "ReportSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_FeaturePermissions_FeatureId_PermissionId",
                table: "FeaturePermissions");

            migrationBuilder.AddColumn<int>(
                name: "RoleId1",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleFeatures_RoleId",
                table: "RoleFeatures",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleFeaturePermissions_RoleId",
                table: "RoleFeaturePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleId1",
                table: "Accounts",
                column: "RoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Roles_RoleId1",
                table: "Accounts",
                column: "RoleId1",
                principalTable: "Roles",
                principalColumn: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Companies_AgentCompanyId",
                table: "Customers",
                column: "AgentCompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Features_Permissions_PermissionId",
                table: "Features",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "PermissionId");

        }
    }
}
