CREATE TABLE [Security].[UserSession]
(
	[SessionID] BIGINT IDENTITY(1,1) NOT NULL, 
    [UserID] UNIQUEIDENTIFIER NOT NULL, 
    [RenewalToken] UNIQUEIDENTIFIER NOT NULL, 
    [CreatedDate] DATETIME CONSTRAINT [DF_Security_UserSession_CreatedDate] DEFAULT (getutcdate()) NOT NULL, 
    [ExpirationDate] DATETIME NULL, 
    [RenewedDate] DATETIME NULL, 
    CONSTRAINT [PK_Security_UserSession] PRIMARY KEY ([SessionID]), 
    CONSTRAINT [FK_Security_UserSession_User] FOREIGN KEY ([UserID]) REFERENCES [Security].[User]([UserID])
)
