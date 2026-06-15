using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookingRatesAndVoucherLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RedeemedDate",
                table: "Vouchers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Vouchers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceUsd",
                table: "Transfers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceUsd",
                table: "Tours",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedeemedDate",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "BasePriceUsd",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "BasePriceUsd",
                table: "Tours");
        }
    }
}
