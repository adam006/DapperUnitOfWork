use master
GO
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Dapper')
BEGIN
    CREATE DATABASE [Dapper]
END 
GO

use Dapper

if exists (select * from sysobjects where id = object_id('dbo.Book'))
    drop table "dbo"."Book"

if exists (select * from sysobjects where id = object_id('dbo.Author'))
    drop table "dbo"."Author"

create table dbo.Author
(
    Id   int identity
        constraint Author_pk
            primary key nonclustered,
    Name varchar(50) not null
)
go

set identity_insert dbo.Author on
go
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (1, N'Francis Sparks');
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (2, N'Ernest Cline');
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (3, N'Billy Bob');
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (4, N'Karen Winkle');
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (5, N'Michael Scott');
INSERT INTO Dapper.dbo.Author (Id, Name) VALUES (6, N'New Author');

set identity_insert dbo.Author off
go

create table dbo.Book
(
    Id       int identity
        constraint Book_pk
            primary key nonclustered,
    Title    varchar(100) not null,
    Price    decimal(10, 2),
    AuthorId int
        constraint Book__Author_fk
            references Dapper.dbo.Author
)
go
set identity_insert dbo.Book on
go

INSERT INTO Dapper.dbo.Book (Id, Title, Price, AuthorId) VALUES (1, N'Ready Player One', 19.99, 2);
INSERT INTO Dapper.dbo.Book (Id, Title, Price, AuthorId) VALUES (2, N'Made Safe', 4.99, 1);
INSERT INTO Dapper.dbo.Book (Id, Title, Price, AuthorId) VALUES (3, N'It isn''t much, but it''s honest work', 9.99, 3);
INSERT INTO Dapper.dbo.Book (Id, Title, Price, AuthorId) VALUES (4, N'How to ask for the manager', 29.99, 4);
INSERT INTO Dapper.dbo.Book (Id, Title, Price, AuthorId) VALUES (5, N'Somehow I manage', 39.50, 5);

set identity_insert dbo.book off
go

CREATE PROCEDURE dbo.uspGetBookByTitle
    @title varchar(100)
AS

    SET NOCOUNT ON;
    SELECT *
    FROM Book
    WHERE Title = @title
GO