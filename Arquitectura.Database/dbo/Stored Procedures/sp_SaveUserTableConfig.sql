-- =============================================
-- sp_SaveUserTableConfig
-- Guarda preferencias de usuario (con auditoría)
-- =============================================
CREATE PROCEDURE [dbo].[sp_SaveUserTableConfig]
    @UserId INT,
    @TableKey NVARCHAR(100),
    @VisibleColumns NVARCHAR(MAX),
    @ActiveFilters NVARCHAR(MAX),
    @FilterValues NVARCHAR(MAX),
    @SortConfig NVARCHAR(MAX),
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Obtener TableId
    DECLARE @TableId INT = (SELECT Id FROM dbo.TableDefinitions WHERE TableKey = @TableKey);
    IF @TableId IS NULL
    BEGIN
        RAISERROR('TableKey not found: %s', 16, 1, @TableKey);
        RETURN;
    END
    
    -- Obtener valores anteriores para auditoría
    DECLARE @OldVisible NVARCHAR(MAX);
    DECLARE @OldActive NVARCHAR(MAX);
    DECLARE @OldFilterValues NVARCHAR(MAX);
    DECLARE @OldSort NVARCHAR(MAX);
    DECLARE @OldPageSize INT;
    
    SELECT 
        @OldVisible = VisibleColumns,
        @OldActive = ActiveFilters,
        @OldFilterValues = FilterValues,
        @OldSort = SortConfig,
        @OldPageSize = PageSize
    FROM dbo.UserTablePreferences
    WHERE UserId = @UserId AND TableId = @TableId;
    
    -- Determinar tipo de cambio
    DECLARE @ChangeType NVARCHAR(20) = CASE WHEN @OldVisible IS NULL THEN 'INSERT' ELSE 'UPDATE' END;
    
    -- MERGE: Insert or Update
    MERGE INTO dbo.UserTablePreferences AS target
    USING (SELECT @UserId AS UserId, @TableId AS TableId) AS source
    ON target.UserId = source.UserId AND target.TableId = source.TableId
    WHEN MATCHED THEN
        UPDATE SET 
            VisibleColumns = @VisibleColumns,
            ActiveFilters = @ActiveFilters,
            FilterValues = @FilterValues,
            SortConfig = @SortConfig,
            PageSize = @PageSize,
            UpdatedAt = GETUTCDATE()
    WHEN NOT MATCHED THEN
        INSERT (UserId, TableId, VisibleColumns, ActiveFilters, FilterValues, SortConfig, PageSize)
        VALUES (@UserId, @TableId, @VisibleColumns, @ActiveFilters, @FilterValues, @SortConfig, @PageSize);
    
    -- Insertar en historial de auditoría
    INSERT INTO dbo.UserTablePreferencesHistory 
        (UserId, TableId, VisibleColumns, ActiveFilters, FilterValues, SortConfig, PageSize, ChangeType)
    VALUES 
        (@UserId, @TableId, @VisibleColumns, @ActiveFilters, @FilterValues, @SortConfig, @PageSize, @ChangeType);
    
    -- Devolver resultado
    SELECT @ChangeType AS ChangeType, GETUTCDATE() AS ChangedAt;
END