-- =============================================
-- UserTablePreferencesHistory: auditoría de cambios
-- =============================================
CREATE TABLE [dbo].[UserTablePreferencesHistory] (
    [Id]              INT            IDENTITY(1,1)  NOT NULL,
    [UserId]           INT            NOT NULL,
    [TableId]          INT            NOT NULL,
    [VisibleColumns]   NVARCHAR(MAX)  NULL,
    [ActiveFilters]    NVARCHAR(MAX)  NULL,
    [FilterValues]     NVARCHAR(MAX)  NULL,
    [SortConfig]      NVARCHAR(MAX)  NULL,
    [PageSize]         INT            NULL,
    [ChangedAt]        DATETIME      DEFAULT(GETUTCDATE()) NOT NULL,
    [ChangeType]      NVARCHAR(20)  NOT NULL,
    CONSTRAINT [PK_UserTablePreferencesHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO