-- =============================================
-- UserTablePreferences: preferencias por usuario
-- =============================================
CREATE TABLE [dbo].[UserTablePreferences] (
    [Id]              INT            IDENTITY(1,1)  NOT NULL,
    [UserId]           INT            NOT NULL,
    [TableId]          INT            NOT NULL,
    [VisibleColumns]   NVARCHAR(MAX)  NULL,
    [ActiveFilters]    NVARCHAR(MAX)  NULL,
    [FilterValues]     NVARCHAR(MAX)  NULL,
    [SortConfig]      NVARCHAR(MAX)  NULL,
    [PageSize]         INT            NULL,
    [UpdatedAt]        DATETIME      DEFAULT(GETUTCDATE()) NULL,
    CONSTRAINT [PK_UserTablePreferences] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserTablePreferences_TableDefinitions] FOREIGN KEY ([TableId]) REFERENCES [dbo].[TableDefinitions]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_UserTablePreferences_UserId_TableId] UNIQUE NONCLUSTERED ([UserId], [TableId])
);
GO