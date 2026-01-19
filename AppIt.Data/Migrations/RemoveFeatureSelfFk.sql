-- Idempotent script to remove erroneous self-referencing FK/index/column from Features table
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Features_Features_FeatureIdId')
BEGIN
    ALTER TABLE [dbo].[Features] DROP CONSTRAINT [FK_Features_Features_FeatureIdId];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Features_FeatureIdId' AND object_id = OBJECT_ID('dbo.Features'))
BEGIN
    DROP INDEX [IX_Features_FeatureIdId] ON [dbo].[Features];
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'FeatureIdId' AND Object_ID = Object_ID(N'dbo.Features'))
BEGIN
    ALTER TABLE [dbo].[Features] DROP COLUMN [FeatureIdId];
END
