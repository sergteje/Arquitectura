-- =============================================
-- Seed: DataTable Metadatos
-- Inserta metadatos de WeatherForecasts y Users
-- =============================================

-- WeatherForecasts
INSERT INTO dbo.TableDefinitions (TableKey, DisplayName, DefaultPageSize)
VALUES ('WeatherForecasts', 'Pronósticos del tiempo', 10);

DECLARE @WFTableId INT = (SELECT SCOPE_IDENTITY());

INSERT INTO dbo.TableColumns (TableId, ColumnKey, DisplayName, DataType, IsSortable, IsFilterable, IsVisibleByDefault, IsFilterVisibleByDefault, IsAlwaysVisible, SortOrder, FilterType)
VALUES 
    (@WFTableId, 'Id', 'ID', 'number', 1, 0, 0, 0, 1, 1, NULL),
    (@WFTableId, 'Date', 'Fecha', 'date', 1, 1, 1, 1, 0, 2, 'daterange'),
    (@WFTableId, 'TemperatureC', 'Temperatura (°C)', 'number', 1, 1, 1, 1, 0, 3, NULL),
    (@WFTableId, 'Summary', 'Resumen', 'string', 1, 1, 1, 1, 0, 4, 'select'),
    (@WFTableId, 'CreatedAt', 'Creado', 'date', 1, 1, 0, 0, 0, 5, 'daterange'),
    (@WFTableId, 'UpdatedAt', 'Actualizado', 'date', 1, 0, 0, 0, 0, 6, NULL),
    (@WFTableId, 'CreatedBy', 'Creado por', 'string', 1, 0, 0, 0, 0, 7, NULL),
    (@WFTableId, 'UpdatedBy', 'Actualizado por', 'string', 1, 0, 0, 0, 0, 8, NULL);

-- Opciones para Summary
UPDATE dbo.TableColumns 
SET FilterOptionsSource = 'SELECT DISTINCT ISNULL(Summary, ''Sin resumen'') AS value, ISNULL(Summary, ''Sin resumen'') AS label FROM WeatherForecasts WHERE Summary IS NOT NULL'
WHERE TableId = @WFTableId AND ColumnKey = 'Summary';

-- Users
INSERT INTO dbo.TableDefinitions (TableKey, DisplayName, DefaultPageSize)
VALUES ('Users', 'Usuarios', 10);

DECLARE @UsersTableId INT = (SELECT SCOPE_IDENTITY());

INSERT INTO dbo.TableColumns (TableId, ColumnKey, DisplayName, DataType, IsSortable, IsFilterable, IsVisibleByDefault, IsFilterVisibleByDefault, IsAlwaysVisible, SortOrder, FilterType)
VALUES 
    (@UsersTableId, 'Id', 'ID', 'number', 1, 0, 0, 0, 1, 1, NULL),
    (@UsersTableId, 'Email', 'Email', 'string', 1, 1, 1, 1, 0, 2, 'text'),
    (@UsersTableId, 'FirstName', 'Nombre', 'string', 1, 1, 1, 1, 0, 3, 'text'),
    (@UsersTableId, 'LastName', 'Apellido', 'string', 1, 1, 1, 1, 0, 4, 'text'),
    (@UsersTableId, 'PhoneNumber', 'Teléfono', 'string', 1, 0, 0, 0, 0, 5, NULL),
    (@UsersTableId, 'IsActive', 'Activo', 'boolean', 1, 1, 1, 1, 0, 6, 'boolean'),
    (@UsersTableId, 'LastLoginAt', 'Último acceso', 'date', 1, 0, 0, 0, 0, 7, NULL),
    (@UsersTableId, 'CreatedAt', 'Creado', 'date', 1, 0, 0, 0, 0, 8, NULL);

PRINT 'Seed completado: WeatherForecasts y Users';
GO