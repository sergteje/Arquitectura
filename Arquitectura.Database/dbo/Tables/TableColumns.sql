-- =============================================
-- TableColumns: metadatos de columnas por tabla
-- =============================================
CREATE TABLE [dbo].[TableColumns] (
    [Id]                       INT            IDENTITY(1,1)  NOT NULL,
    [TableId]                   INT            NOT NULL,
    [ColumnKey]                 NVARCHAR(100)  NOT NULL,
    [DisplayName]              NVARCHAR(200)  NOT NULL,
    [DataType]                  NVARCHAR(50)   NOT NULL,
    [IsSortable]               BIT            DEFAULT(1)   NOT NULL,
    [IsFilterable]             BIT            DEFAULT(1)   NOT NULL,
    [IsVisibleByDefault]        BIT            DEFAULT(1)   NOT NULL,
    [IsFilterVisibleByDefault] BIT            DEFAULT(1)   NOT NULL,
    [IsAlwaysVisible]          BIT            DEFAULT(0)   NOT NULL,
    [SortOrder]                INT            DEFAULT(0)   NOT NULL,
    [Width]                    INT            NULL,
    [FilterType]               NVARCHAR(50)   NULL,
    [FilterOptionsSource]       NVARCHAR(200)  NULL,
    [CreatedAt]                DATETIME      DEFAULT(GETUTCDATE()) NULL,
    CONSTRAINT [PK_TableColumns] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TableColumns_TableDefinitions] FOREIGN KEY ([TableId]) REFERENCES [dbo].[TableDefinitions]([Id]) ON DELETE CASCADE
);
GO