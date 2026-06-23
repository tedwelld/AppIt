SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DELETE FROM ProofOfPayments;
DELETE FROM Refunds;
DELETE FROM CreditNotes;
DELETE FROM Commissions;
DELETE FROM Vouchers;
DELETE FROM ReservationServiceItems;
DELETE FROM ReservationSnapshots;
DELETE FROM Payments;
DELETE FROM Invoices;
DELETE FROM Reservations;
DELETE FROM SpecialProductPrices;
DELETE FROM ServicePrices;
DELETE FROM Products;
DELETE FROM Activities;
DELETE FROM Tours;
DELETE FROM Transfers;
DELETE FROM Accommodations;
DELETE FROM ProductSubCategories;
DELETE FROM ProductCategories;
DELETE FROM CustomerTypes;
DELETE FROM Customers;
DELETE FROM Consultants;
DELETE FROM SupportMessages;
DELETE FROM Notifications;
DELETE FROM AuditLogs;
DELETE FROM ReportSnapshots;
DELETE FROM DayEnds;
DELETE FROM IdempotencyRecords;
DELETE FROM ExchangeRates;
DELETE FROM Currencies;
DELETE FROM Suppliers;
DELETE FROM Companies;
DELETE FROM Departments;
DELETE FROM RefreshTokens WHERE AccountId NOT IN (SELECT Id FROM Accounts WHERE LOWER(Email) = 'admin@appit.com');
DELETE FROM PasswordResetTokens WHERE AccountId NOT IN (SELECT Id FROM Accounts WHERE LOWER(Email) = 'admin@appit.com');
DELETE FROM Accounts WHERE LOWER(Email) <> 'admin@appit.com';

SELECT 'Products' AS [Table], COUNT(*) AS [Rows] FROM Products
UNION ALL SELECT 'ProductCategories', COUNT(*) FROM ProductCategories
UNION ALL SELECT 'Reservations', COUNT(*) FROM Reservations
UNION ALL SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL SELECT 'Accounts', COUNT(*) FROM Accounts
UNION ALL SELECT 'Companies', COUNT(*) FROM Companies;
