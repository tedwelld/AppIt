using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationCapacityAndServicePricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuestCapacity",
                table: "Accommodations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ServicePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO ServicePrices (ServiceType, ServiceId, CurrencyCode, UnitPrice, IsActive, CreatedAt)
                SELECT 'Product', ProductId, 'USD', BasePriceUsd, CAST(1 AS bit), SYSUTCDATETIME()
                FROM Products
                WHERE BasePriceUsd > 0;

                INSERT INTO ServicePrices (ServiceType, ServiceId, CurrencyCode, UnitPrice, IsActive, CreatedAt)
                SELECT 'Accommodation', Id, 'USD', BasePriceUsd, CAST(1 AS bit), SYSUTCDATETIME()
                FROM Accommodations
                WHERE BasePriceUsd > 0;

                INSERT INTO ServicePrices (ServiceType, ServiceId, CurrencyCode, UnitPrice, IsActive, CreatedAt)
                SELECT 'Activity', Id, 'USD', BasePriceUsd, CAST(1 AS bit), SYSUTCDATETIME()
                FROM Activities
                WHERE BasePriceUsd > 0;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePrices_ServiceType_ServiceId_CurrencyCode",
                table: "ServicePrices",
                columns: new[] { "ServiceType", "ServiceId", "CurrencyCode" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePrices");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropColumn(
                name: "GuestCapacity",
                table: "Accommodations");
        }
    }
}
