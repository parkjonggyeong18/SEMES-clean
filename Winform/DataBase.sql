
CREATE DATABASE MyCompanyTest
 COLLATE Korean_100_CI_AS;     
GO
USE MyCompanyTest;
GO


CREATE TABLE dbo.Users (        -- 사원(사용자) 정보
    id             INT IDENTITY(1,1)  PRIMARY KEY,    -- 사번(PK)
    userName       NVARCHAR(100) UNIQUE NOT NULL,      -- 로그인 ID
    [password]     NVARCHAR(100)        NOT NULL,      -- 비밀번호
    firstName      NVARCHAR(100)        NOT NULL,      -- 이름
    lastName       NVARCHAR(100)        NOT NULL,      -- 성
    [position]     NVARCHAR(100)        NOT NULL,      -- 직책
    email          NVARCHAR(100) UNIQUE NOT NULL,      -- 이메일
    profilePicture VARBINARY(MAX) NULL                -- 프로필 사진
);
GO


CREATE PROC dbo.AddUser
    @userName  NVARCHAR(100),
    @password  NVARCHAR(100),
    @firstName NVARCHAR(100),
    @lastName  NVARCHAR(100),
    @position  NVARCHAR(100),
    @email     NVARCHAR(100),
    @photo     VARBINARY(MAX)
AS
INSERT INTO dbo.Users
VALUES (@userName, @password, @firstName, @lastName, @position, @email, @photo);
GO

CREATE PROC dbo.EditUser
    @userName  NVARCHAR(100),
    @password  NVARCHAR(100),
    @firstName NVARCHAR(100),
    @lastName  NVARCHAR(100),
    @position  NVARCHAR(100),
    @email     NVARCHAR(100),
    @photo     VARBINARY(MAX),
    @id        INT
AS
UPDATE dbo.Users
SET userName       = @userName,
    [password]     = @password,
    firstName      = @firstName,
    lastName       = @lastName,
    [position]     = @position,
    email          = @email,
    profilePicture = @photo
WHERE id = @id;
GO

CREATE PROC dbo.RemoveUser
    @id INT
AS
DELETE FROM dbo.Users WHERE id = @id;
GO

CREATE PROC dbo.LoginUser
    @user     NVARCHAR(100),
    @password NVARCHAR(100)
AS
SELECT *
FROM   dbo.Users
WHERE  ((userName = @user OR email = @user) AND [password] = @password);
GO

CREATE PROC dbo.SelectAllUsers
AS
SELECT * FROM dbo.Users;
GO

CREATE PROC dbo.SelectUser
    @findValue NVARCHAR(100)
AS
SELECT *
FROM   dbo.Users
WHERE  userName  = @findValue
   OR  firstName LIKE @findValue + N'%'
   OR  email     = @findValue;
GO


EXEC dbo.AddUser
    N'admin',  N'admin',
    N'홍',     N'길동',
    N'시스템 관리자',
    N'admin@mycompany.kr',
    NULL;

EXEC dbo.AddUser
    N'kimmj',  N'Pa$$w0rd',
    N'민지',   N'김',
    N'회계 담당',
    N'kim.mj@mycompany.kr',
    NULL;

EXEC dbo.AddUser
    N'leehr',  N'Pa$$w0rd',
    N'하라',   N'이',
    N'총무',
    N'lee.hr@mycompany.kr',
    NULL;
GO
