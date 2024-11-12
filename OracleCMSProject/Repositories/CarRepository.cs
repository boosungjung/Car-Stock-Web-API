using System.Data;
using Microsoft.EntityFrameworkCore;
using OracleCMSProject.Models;
using Dapper;
namespace OracleCMSProject.Repositories;

public class CarRepository: ICarRepository
{
  private readonly IDbConnection _connection;

    public CarRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    // Retrieve all cars for a specific dealer
    public async Task<IEnumerable<Car>> GetAllCars(int dealerId)
    {
        var query = "SELECT * FROM Cars WHERE DealerId = @DealerId";
        return await _connection.QueryAsync<Car>(query, new { DealerId = dealerId });
    }

    // Retrieve a car by ID for a specific dealer
    public async Task<Car?> GetCarById(int id, int dealerId)
    {
        var query = "SELECT * FROM Cars WHERE Id = @Id AND DealerId = @DealerId";
        return await _connection.QuerySingleOrDefaultAsync<Car>(query, new { Id = id, DealerId = dealerId });
    }

    // Add a new car
    public async Task AddCar(Car car)
    {
        var query = @"
            INSERT INTO Cars (Make, Model, Year, StockLevel, DealerId)
            VALUES (@Make, @Model, @Year, @StockLevel, @DealerId)";
        await _connection.ExecuteAsync(query, car);
    }

    // Update an existing car's information
    public async Task UpdateCar(Car car)
    {
        var query = @"
            UPDATE Cars 
            SET Make = @Make, Model = @Model, Year = @Year, StockLevel = @StockLevel
            WHERE Id = @Id AND DealerId = @DealerId";
        await _connection.ExecuteAsync(query, car);
    }

    // Delete a car by ID for a specific dealer
    public async Task DeleteCar(int id, int dealerId)
    {
        var query = "DELETE FROM Cars WHERE Id = @Id AND DealerId = @DealerId";
        await _connection.ExecuteAsync(query, new { Id = id, DealerId = dealerId });
    }

    // Update car stock level for a specific car and dealer
    public async Task UpdateCarStock(int id, int stockLevel, int dealerId)
    {
        var query = "UPDATE Cars SET StockLevel = @StockLevel WHERE Id = @Id AND DealerId = @DealerId";
        await _connection.ExecuteAsync(query, new { Id = id, StockLevel = stockLevel, DealerId = dealerId });
    }

    // Search cars by make and model for a specific dealer
    public async Task<IEnumerable<Car>> SearchCars(string make, string model, int dealerId)
    {
        var query = @"
            SELECT * FROM Cars
            WHERE DealerId = @DealerId AND
                  Make LIKE @Make AND
                  Model LIKE @Model";
        return await _connection.QueryAsync<Car>(query, new { DealerId = dealerId, Make = $"%{make}%", Model = $"%{model}%" });
    }
}