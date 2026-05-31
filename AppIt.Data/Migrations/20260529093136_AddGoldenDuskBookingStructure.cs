using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoldenDuskBookingStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivityDate",
                table: "ReservationServiceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdultPax",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildPax",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompPax",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostOfSale",
                table: "ReservationServiceItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "ReservationServiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropoffLocation",
                table: "ReservationServiceItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Nights",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ReservationServiceItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupLocation",
                table: "ReservationServiceItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rooms",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercent",
                table: "ReservationServiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Reservations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TravelStatus",
                table: "Reservations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ReservationSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    SnapshotType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationSnapshots_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSnapshots_ReservationId",
                table: "ReservationSnapshots",
                column: "ReservationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationSnapshots");

            migrationBuilder.DropColumn(
                name: "ActivityDate",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "AdultPax",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "ChildPax",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "CompPax",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "CostOfSale",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "DropoffLocation",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "Nights",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "PickupLocation",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "VatPercent",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TravelStatus",
                table: "Reservations");
        }
    }
}
