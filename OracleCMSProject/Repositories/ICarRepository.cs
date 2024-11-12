using OracleCMSProject.Models;

namespace OracleCMSProject.Repositories;

public interface ICarRepository
{
    // Retrieve all cars for a specific dealer
    Task<IEnumerable<Car>> GetAllCars(int dealerId);

    // Retrieve a specific car by ID for a specific dealer
    Task<Car?> GetCarById(int id, int dealerId);

    // Add a new car
    Task AddCar(Car car);

    // Update an existing car
    Task UpdateCar(Car car);

    // Delete a car by ID for a specific dealer
    Task DeleteCar(int id, int dealerId);
    
    
    // Update car stock level for a specific car and dealer
    Task UpdateCarStock(int id, int stockLevel, int dealerId);
    
    // Search for cars by make, model, and year

    Task<IEnumerable<Car>> SearchCars(string make, string model, int dealerId);
}