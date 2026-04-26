CREATE TABLE [dbo].[WeatherForecasts] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Date]         DATETIME       NOT NULL,
    [TemperatureC] INT            NOT NULL,
    [Summary]      NVARCHAR (500) NULL,
    [CreatedAt]    DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]    DATETIME       NULL,
    [CreatedBy]    NVARCHAR (100) DEFAULT ('system') NULL,
    [UpdatedBy]    NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

