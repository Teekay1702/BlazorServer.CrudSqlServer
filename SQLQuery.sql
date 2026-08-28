CREATE DATABASE BlazorCrudDb;
GO

USE BlazorCrudDb;

CREATE TABLE Employees (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Email NVARCHAR(100),
    Position NVARCHAR(100)
);