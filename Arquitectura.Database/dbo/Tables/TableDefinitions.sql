-- =============================================
-- TableDefinitions: define las entidades dinámicas
-- =============================================
CREATE TABLE [dbo].[TableDefinitions] (
    [Id]              INT            IDENTITY(1,1)  NOT NULL,
    [TableKey]         NVARCHAR(100)  NOT NULL,
    [DisplayName]      NVARCHAR(200)  NOT NULL,
    [DefaultPageSize] INT            DEFAULT(10)  NOT NULL,
    [CreatedAt]       DATETIME      DEFAULT(GETUTCDATE()) NULL,
    CONSTRAINT [PK_TableDefinitions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_TableDefinitions_TableKey] UNIQUE NONCLUSTERED ([TableKey] ASC)
);
GO