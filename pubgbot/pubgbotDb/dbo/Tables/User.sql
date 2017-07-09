CREATE TABLE [dbo].[User] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [DiscordName] NVARCHAR (150) NULL,
    [SteamId]     NVARCHAR (150) NULL,
    [Location]    NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);



