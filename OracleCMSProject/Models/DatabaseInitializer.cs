using System.Data;
using Dapper;

namespace OracleCMSProject.Models;

public static class DatabaseInitializer
{
    public static void Initialize(IDbConnection connection)
    {
        var createUsersTableQuery = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            );";

        var createCarsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Cars (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Make TEXT NOT NULL,
                Model TEXT NOT NULL,
                Year INTEGER NOT NULL,
                StockLevel INTEGER NOT NULL,
                DealerId INTEGER NOT NULL,
                FOREIGN KEY (DealerId) REFERENCES Users(Id) ON DELETE CASCADE
            );";

        connection.Execute(createUsersTableQuery);
        connection.Execute(createCarsTableQuery);
    }
}