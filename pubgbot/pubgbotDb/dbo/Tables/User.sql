CREATE TABLE [dbo].[User] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [DiscordName] NVARCHAR (150) NULL,
    [SteamId]     NVARCHAR (150) NULL,
    [Region]      INT            NOT NULL,
    CONSTRAINT [PK_dbo.User] PRIMARY KEY CLUSTERED ([Id] ASC)
);





