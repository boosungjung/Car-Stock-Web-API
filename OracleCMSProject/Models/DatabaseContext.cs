using System.Data;
using Microsoft.Data.Sqlite;

namespace OracleCMSProject.Models;

public class DatabaseContext
{
    private readonly string _connectionString = "Data Source=dealerCars.db";

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}