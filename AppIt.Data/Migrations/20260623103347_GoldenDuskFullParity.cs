using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppIt.Data.Migrations
{
    /// <inheritdoc />
    public partial class GoldenDuskFullParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalKey",
                table: "SpecialProductPrices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "SpecialProductPrices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAgentApproved",
                table: "SpecialProductPrices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "SpecialProductPrices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "SpecialProductPrices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sent",
                table: "SpecialProductPrices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedOn",
                table: "SpecialProductPrices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "ReservationServiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPostedToJournal",
                table: "ReservationServiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MiscCode",
                table: "ReservationServiceItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductKind",
                table: "ReservationServiceItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFiscalized",
                table: "Refunds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalOriginalReceiptNo",
                table: "Refunds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalQrCodeUrl",
                table: "Refunds",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FiscalReceiptNo",
                table: "Refunds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFiscalized",
                table: "Refunds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProviderRefundId",
                table: "Refunds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFiscalized",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalCisInvoiceNo",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalQrCodeUrl",
                table: "Invoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FiscalReceiptNo",
                table: "Invoices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalSdcId",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFiscalized",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadyToRefiscalize",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RefiscalizeReservationSnapShot",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReservationSnapShot",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Companies",
                type: "decimal(12,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreditAgent",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AgentProductPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    NetRate = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: true),
                    RackRate = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsAgentApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false),
                    Query = table.Column<bool>(type: "bit", nullable: false),
                    YearEffected = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    QueryNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovalKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentProductPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentProductPrices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BankNoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashUpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Denomination = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    EnteredBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankNoteDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: true),
                    MaxProducts = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combos_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Combos_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CreditMemos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    CreditNoteId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditMemos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditMemos_CreditNotes_CreditNoteId",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemos_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditMemos_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PastelRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HConnectBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    HConnectPropertyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HConnectRoomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HConnectConfirmationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SyncStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ArrivalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GuestFirstName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GuestLastName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastSyncAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HConnectBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HConnectBookings_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HConnectProductMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccommodationId = table.Column<int>(type: "int", nullable: false),
                    HConnectRoomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HConnectPropertyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HConnectProductMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HConnectProductMappings_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoucherReference = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    JournalType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservationServiceItemSplits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationServiceItemId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationServiceItemSplits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationServiceItemSplits_ReservationServiceItems_ReservationServiceItemId",
                        column: x => x.ReservationServiceItemId,
                        principalTable: "ReservationServiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VsdcCodeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VsdcCodeEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VsdcDeviceInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SdcId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MrcNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsInitialized = table.Column<bool>(type: "bit", nullable: false),
                    InitializedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VsdcDeviceInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComboPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboPrices_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboProducts_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 18, scale: 2, nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServiceItems_ComboId",
                table: "ReservationServiceItems",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentProductPrices_CompanyId",
                table: "AgentProductPrices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboPrices_ComboId",
                table: "ComboPrices",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboProducts_ComboId",
                table: "ComboProducts",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_Code",
                table: "Combos",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_ProductCategoryId",
                table: "Combos",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_SupplierId",
                table: "Combos",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_CreditNoteId",
                table: "CreditMemos",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_InvoiceId",
                table: "CreditMemos",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemos_ReservationId",
                table: "CreditMemos",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_HConnectBookings_ReservationId",
                table: "HConnectBookings",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_HConnectProductMappings_AccommodationId",
                table: "HConnectProductMappings",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReservationId",
                table: "JournalEntries",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_FinancialAccountId",
                table: "JournalEntryLines",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServiceItemSplits_ReservationServiceItemId",
                table: "ReservationServiceItemSplits",
                column: "ReservationServiceItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationServiceItems_Combos_ComboId",
                table: "ReservationServiceItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationServiceItems_Combos_ComboId",
                table: "ReservationServiceItems");

            migrationBuilder.DropTable(
                name: "AgentProductPrices");

            migrationBuilder.DropTable(
                name: "BankNoteDetails");

            migrationBuilder.DropTable(
                name: "ComboPrices");

            migrationBuilder.DropTable(
                name: "ComboProducts");

            migrationBuilder.DropTable(
                name: "CreditMemos");

            migrationBuilder.DropTable(
                name: "HConnectBookings");

            migrationBuilder.DropTable(
                name: "HConnectProductMappings");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "ReservationServiceItemSplits");

            migrationBuilder.DropTable(
                name: "VsdcCodeEntries");

            migrationBuilder.DropTable(
                name: "VsdcDeviceInfos");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReservationServiceItems_ComboId",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "ApprovalKey",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "IsAgentApproved",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "Sent",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "VerifiedOn",
                table: "SpecialProductPrices");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "IsPostedToJournal",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "MiscCode",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "ProductKind",
                table: "ReservationServiceItems");

            migrationBuilder.DropColumn(
                name: "DateFiscalized",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "FiscalOriginalReceiptNo",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "FiscalQrCodeUrl",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "FiscalReceiptNo",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "IsFiscalized",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "PaymentProviderRefundId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "DateFiscalized",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FiscalCisInvoiceNo",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FiscalQrCodeUrl",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FiscalReceiptNo",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "FiscalSdcId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsFiscalized",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsReadyToRefiscalize",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RefiscalizeReservationSnapShot",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReservationSnapShot",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsCreditAgent",
                table: "Companies");
        }
    }
}
