CREATE TABLE [dbo].[Stats] (
    [Id]           INT           NOT NULL,
    [UserId]       NVARCHAR (50) NULL,
    [Region]       NVARCHAR (50) NULL,
    [Category]     NVARCHAR (50) NULL,
    [Field]        NVARCHAR (50) NULL,
    [DisplayValue] NVARCHAR (50) NULL,
    [Value]        NVARCHAR (50) NULL,
    [ValueInt]     INT           NULL,
    [ValueDec]     DECIMAL (18)  NULL,
    [Rank]         INT           NULL,
    [Percentile]   DECIMAL (18)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

