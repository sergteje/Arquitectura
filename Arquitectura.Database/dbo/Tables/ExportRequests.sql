-- =============================================
-- ExportRequests: solicitudes de exportación
-- =============================================
CREATE TABLE [dbo].[ExportRequests] (
    [Id]              INT            IDENTITY(1,1)  NOT NULL,
    [RequestId]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]           INT            NOT NULL,
    [TableKey]         NVARCHAR(100)  NOT NULL,
    [FiltersJson]     NVARCHAR(MAX)  NULL,
    [SortColumn]       NVARCHAR(100)  NULL,
    [SortDirection]    NVARCHAR(4)   NULL,
    [Status]           NVARCHAR(20)  NOT NULL,
    [FilePath]        NVARCHAR(500)  NULL,
    [FileName]        NVARCHAR(200)  NULL,
    [ErrorMessage]    NVARCHAR(MAX)  NULL,
    [CreatedAt]        DATETIME      DEFAULT(GETUTCDATE()) NOT NULL,
    [CompletedAt]     DATETIME      NULL,
    [ExpiresAt]        DATETIME      NULL,
    CONSTRAINT [PK_ExportRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_ExportRequests_RequestId] UNIQUE NONCLUSTERED ([RequestId] ASC)
);
GO