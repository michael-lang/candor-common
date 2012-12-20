CREATE TABLE [Security].[User]
(
    [RecordID]            INT              IDENTITY (1, 1) NOT NULL,
    [UserID]              UNIQUEIDENTIFIER NOT NULL,
    [Name]                NVARCHAR (200)   NOT NULL,
    [PasswordHash]        NVARCHAR (2048)   NOT NULL,
    [PasswordHashUpdatedDate] DATETIME         NULL,
    [PasswordUpdatedDate] DATETIME         NULL,
    [IsDeleted]           BIT              CONSTRAINT [DF_Security_User_IsDeleted] DEFAULT ((0)) NOT NULL,
    [CreatedDate]         DATETIME         CONSTRAINT [DF_Security_User_CreatedDate] DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserID]     UNIQUEIDENTIFIER NULL,
    [UpdatedDate]         DATETIME         NULL,
    [UpdatedByUserID]     UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Security_User] PRIMARY KEY NONCLUSTERED ([UserID]), 
    CONSTRAINT [AK_Security_User_RecordID] UNIQUE CLUSTERED ([RecordID]), 
    CONSTRAINT [FK_Security_User_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Security].[User]([UserID]), 
    CONSTRAINT [FK_Security_User_UpdatedByUserID] FOREIGN KEY ([UpdatedByUserID]) REFERENCES [Security].[User]([UserID]), 
    CONSTRAINT [AK_Security_User_UserID] UNIQUE ([UserID])
)
