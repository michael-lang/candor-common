CREATE TABLE [Security].[AuthenticationHistory]
(
	[RecordID] BIGINT IDENTITY(1,1) NOT NULL, 
    [UserName] NVARCHAR(1024) NOT NULL, 
    [IPAddress] VARCHAR(30) NULL, 
    [CreatedDate] DATETIME CONSTRAINT [DF_Security_AuthenticationHistory_CreatedDate] DEFAULT (getutcdate()) NOT NULL, 
    [IsAuthenticated] BIT CONSTRAINT [DF_Security_AuthenticationHistory_IsAuthenticated] DEFAULT (0) NULL, 
    [SessionID] BIGINT NULL, 
    CONSTRAINT [PK_Security_AuthenticationHistory] PRIMARY KEY ([RecordID]), 
    CONSTRAINT [FK_Security_AuthenticationHistory_UserSession] FOREIGN KEY ([SessionID]) REFERENCES [Security].[UserSession]([SessionID])
)
