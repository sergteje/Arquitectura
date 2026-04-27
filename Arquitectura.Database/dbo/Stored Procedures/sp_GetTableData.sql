-- =============================================
-- sp_GetTableData
-- Consulta dinámica con filtros, orden y paginación
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetTableData]
    @TableKey NVARCHAR(100),
    @Page INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(100) = NULL,
    @SortDirection NVARCHAR(4) = 'ASC',
    @FiltersJson NVARCHAR(MAX) = '{}'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar tabla
    DECLARE @TableId INT = (SELECT Id FROM dbo.TableDefinitions WHERE TableKey = @TableKey);
    IF @TableId IS NULL
    BEGIN
        RAISERROR('TableKey not found: %s', 16, 1, @TableKey);
        RETURN;
    END
    
    -- Obtener metadatos de columnas para validar tipos
    CREATE TABLE #ColumnMeta (
        ColumnKey NVARCHAR(100),
        DataType NVARCHAR(50)
    );
    INSERT INTO #ColumnMeta (ColumnKey, DataType)
    SELECT ColumnKey, DataType FROM dbo.TableColumns WHERE TableId = @TableId;
    
    -- Construir WHERE dinámico base64 simple (evitamos SQL dinámico complejo)
    DECLARE @WhereClause NVARCHAR(MAX) = '';
    DECLARE @Counter INT = 1;
    
    -- Parsear filtros JSON simple (solo soporta equality por ahora)
    IF ISJSON(@FiltersJson) = 1
    BEGIN
        DECLARE @key NVARCHAR(100), @val NVARCHAR(MAX), @type NVARCHAR(50);
        DECLARE cur CURSOR FOR
            SELECT f.[key], JSON_VALUE(@FiltersJson, CONCAT('$.', f.[key])), cm.DataType
            FROM OPENJSON(@FiltersJson) f
            INNER JOIN #ColumnMeta cm ON cm.ColumnKey = f.[key]
            WHERE f.[value] IS NOT NULL 
              AND JSON_VALUE(@FiltersJson, CONCAT('$.', f.[key])) IS NOT NULL;
        OPEN cur;
        FETCH NEXT FROM cur INTO @key, @val, @type;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF @type = 'string'
            BEGIN
                SET @WhereClause = @WhereClause + ' AND ' + QUOTENAME(@key) + ' LIKE @p' + CAST(@Counter AS NVARCHAR);
            END
            ELSE IF @type IN ('number', 'boolean')
            BEGIN
                SET @WhereClause = @WhereClause + ' AND ' + QUOTENAME(@key) + ' = @p' + CAST(@Counter AS NVARCHAR);
            END
            
            SET @Counter = @Counter + 1;
            FETCH NEXT FROM cur INTO @key, @val, @type;
        END
        CLOSE cur;
        DEALLOCATE cur;
    END
    
    -- Paginación
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    
    -- Ordenamiento válido
    DECLARE @OrderBy NVARCHAR(MAX) = 'ORDER BY (SELECT NULL)';
    IF @SortColumn IS NOT NULL AND EXISTS (SELECT 1 FROM #ColumnMeta WHERE ColumnKey = @SortColumn)
    BEGIN
        SET @OrderBy = QUOTENAME(@SortColumn) + ' ' + CASE WHEN UPPER(@SortDirection) = 'DESC' THEN 'DESC' ELSE 'ASC' END;
    END
    
    -- Construir y ejecutar consulta
    DECLARE @Sql NVARCHAR(MAX) = N'
        SELECT * FROM ' + QUOTENAME(@TableKey) + N' 
        WHERE 1=1 ' + @WhereClause + N'
        ' + @OrderBy + N'
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        SELECT COUNT(*) FROM ' + QUOTENAME(@TableKey) + N' 
        WHERE 1=1 ' + @WhereClause + N';';
    
    -- Ejecutar con sp_executesql
    EXEC sp_executesql @Sql, 
        N'@Offset INT, @PageSize INT', 
        @Offset = @Offset, @PageSize = @PageSize;
    
    DROP TABLE #ColumnMeta;
END