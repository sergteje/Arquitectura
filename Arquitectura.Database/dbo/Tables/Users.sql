CREATE TABLE [dbo].[Users] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [Email]                 NVARCHAR (255) NOT NULL,
    [PasswordHash]          NVARCHAR (500) NOT NULL,
    [FirstName]             NVARCHAR (100) NOT NULL,
    [LastName]              NVARCHAR (100) NOT NULL,
    [PhoneNumber]           NVARCHAR (50)  NULL,
    [EmailConfirmed]        BIT            DEFAULT ((0)) NULL,
    [IsActive]              BIT            DEFAULT ((1)) NULL,
    [LastLoginAt]           DATETIME       NULL,
    [RefreshToken]          NVARCHAR (500) NULL,
    [RefreshTokenExpiresAt] DATETIME       NULL,
    [CreatedAt]             DATETIME       DEFAULT (getdate()) NULL,
    [UpdatedAt]             DATETIME       NULL,
    [CreatedBy]             NVARCHAR (100) DEFAULT ('system') NULL,
    [UpdatedBy]             NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

