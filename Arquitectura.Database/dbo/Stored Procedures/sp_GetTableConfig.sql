-- =============================================
-- sp_GetTableConfig
-- Obtiene configuración de tabla + preferencias de usuario
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetTableConfig]
    @TableKey NVARCHAR(100),
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Obtener la definición de la tabla
    DECLARE @TableId INT;
    DECLARE @DefaultPageSize INT;
    
    SELECT @TableId = Id, @DefaultPageSize = ISNULL(DefaultPageSize, 10)
    FROM dbo.TableDefinitions
    WHERE TableKey = @TableKey;
    
    IF @TableId IS NULL
    BEGIN
        RAISERROR('TableKey not found: %s', 16, 1, @TableKey);
        RETURN;
    END

    -- 2. Obtener todas las columnas de la tabla
    SELECT 
        ColumnKey,
        DisplayName,
        DataType,
        IsSortable,
        IsFilterable,
        IsVisibleByDefault,
        IsFilterVisibleByDefault,
        IsAlwaysVisible,
        SortOrder,
        Width,
        FilterType,
        FilterOptionsSource
    FROM dbo.TableColumns
    WHERE TableId = @TableId
    ORDER BY SortOrder;

    -- 3. Obtener preferencias del usuario (si existen)
    SELECT 
        VisibleColumns,
        ActiveFilters,
        FilterValues,
        SortConfig,
        PageSize
    FROM dbo.UserTablePreferences
    WHERE UserId = @UserId AND TableId = @TableId;

    -- 4. Devolver PageSize por defecto
    SELECT @DefaultPageSize AS DefaultPageSize;
END