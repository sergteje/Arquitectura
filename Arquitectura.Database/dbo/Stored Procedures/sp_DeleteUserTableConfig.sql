-- =============================================
-- sp_DeleteUserTableConfig
-- Deletes a user's table preferences and records the change in the audit history
-- =============================================
CREATE PROCEDURE [dbo].[sp_DeleteUserTableConfig]
    @UserId INT,
    @TableKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Resolve the table
    DECLARE @TableId INT = (SELECT Id FROM dbo.TableDefinitions WHERE TableKey = @TableKey);
    IF @TableId IS NULL
    BEGIN
        RAISERROR('TableKey not found: %s', 16, 1, @TableKey);
        RETURN;
    END

    -- Audit the values being removed
    INSERT INTO dbo.UserTablePreferencesHistory
        (UserId, TableId, VisibleColumns, ActiveFilters, FilterValues, SortConfig, PageSize, ChangeType)
    SELECT
        UserId, TableId, VisibleColumns, ActiveFilters, FilterValues, SortConfig, PageSize, 'DELETE'
    FROM dbo.UserTablePreferences
    WHERE UserId = @UserId AND TableId = @TableId;

    -- Delete the preferences (a no-op when nothing exists)
    DELETE FROM dbo.UserTablePreferences
    WHERE UserId = @UserId AND TableId = @TableId;

    -- Report the outcome
    SELECT @@ROWCOUNT AS DeletedRows, GETUTCDATE() AS DeletedAt;
END
