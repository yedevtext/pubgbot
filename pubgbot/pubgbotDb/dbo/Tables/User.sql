CREATE TABLE [dbo].[User] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (150) NULL,
    [SteamId] NVARCHAR (150) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

