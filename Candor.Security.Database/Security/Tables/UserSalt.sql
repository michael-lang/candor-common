CREATE TABLE [Security].[UserSalt]
(
    [RecordID]            INT              IDENTITY (1, 1) NOT NULL,
    [UserID]              UNIQUEIDENTIFIER NOT NULL,
    [PasswordSalt]        NVARCHAR (2048)   NOT NULL,
	[ResetCode] VARCHAR(34) NULL, 
    [ResetCodeExpiration] DATETIME NULL, 
    [HashGroup] INT NOT NULL CONSTRAINT [DF_Security_UserSalt_HashGroup] DEFAULT (0), 
    /*Any other user salts or keys go here*/
    CONSTRAINT [PK_Security_UserSalt] PRIMARY KEY NONCLUSTERED ([UserID]), 
    CONSTRAINT [AK_Security_UserSalt_RecordID] UNIQUE CLUSTERED ([RecordID]), 
    CONSTRAINT [FK_Security_UserSalt_User] FOREIGN KEY ([UserID]) REFERENCES [Security].[User]([UserID])
)
