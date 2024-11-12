using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OracleCMSProject.Models;
using OracleCMSProject.Repositories;

namespace OracleCMSProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarRepository _carRepository;

        public CarController(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        private int GetDealerId()
        {
            if (User.Identity is ClaimsIdentity identity)
            {
                var dealerIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (dealerIdClaim != null && int.TryParse(dealerIdClaim.Value, out int dealerId))
                {
                    return dealerId;
                }
            }
            throw new UnauthorizedAccessException("Invalid or missing Dealer ID in claims.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars()
        {
            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var cars = await _carRepository.GetAllCars(dealerId);
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarById(int id)
        {
            if (id <= 0)
                return BadRequest("Car ID must be a positive number.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var car = await _carRepository.GetCarById(id, dealerId);
            if (car == null) return NotFound("Car not found.");
            return Ok(car);
        }

        [HttpPost]
        public async Task<IActionResult> AddCar([FromBody] Car car)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(car.Make))
                return BadRequest("Car make is required.");
            if (string.IsNullOrWhiteSpace(car.Model))
                return BadRequest("Car model is required.");
            if (car.Year <= 0 || car.Year > DateTime.Now.Year)
                return BadRequest($"Car year must be between 1 and {DateTime.Now.Year}.");
            if (car.StockLevel < 0)
                return BadRequest("Stock level cannot be negative.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            car.DealerId = dealerId; // Assign dealerId to the car
            
            await _carRepository.AddCar(car);
            return CreatedAtAction(nameof(GetCarById), new { id = car.Id }, car);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id <= 0)
                return BadRequest("Car ID must be a positive number.");
            if (id != car.Id)
                return BadRequest("Route ID and Car ID do not match.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims

            // Ensure the car belongs to the current dealer
            var existingCar = await _carRepository.GetCarById(id, dealerId);
            if (existingCar == null)
            {
                return NotFound("Car not found or you are not authorized to update this car.");
            }

            if (string.IsNullOrWhiteSpace(car.Make))
                return BadRequest("Car make is required.");
            if (string.IsNullOrWhiteSpace(car.Model))
                return BadRequest("Car model is required.");
            if (car.Year <= 0 || car.Year > DateTime.Now.Year)
                return BadRequest($"Car year must be between 1 and {DateTime.Now.Year}.");
            if (car.StockLevel < 0)
                return BadRequest("Stock level cannot be negative.");

            // Update the car with data from the request
            existingCar.Make = car.Make;
            existingCar.Model = car.Model;
            existingCar.Year = car.Year;
            existingCar.StockLevel = car.StockLevel;

            await _carRepository.UpdateCar(existingCar);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            if (id <= 0)
                return BadRequest("Car ID must be a positive number.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var existingCar = await _carRepository.GetCarById(id, dealerId);
            if (existingCar == null)
            {
                return NotFound("Car not found or you are not authorized to delete this car.");
            }
            await _carRepository.DeleteCar(id, dealerId);
            return NoContent();
        }
        
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateCarStock(int id, [FromBody] int stockLevel)
        {
            if (id <= 0)
                return BadRequest("Car ID must be a positive number.");
            if (stockLevel < 0)
                return BadRequest("Stock level cannot be negative.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims

            // Ensure the car belongs to the current dealer
            var existingCar = await _carRepository.GetCarById(id, dealerId);
            if (existingCar == null)
            {
                return NotFound("Car not found or you are not authorized to update this car.");
            }

            await _carRepository.UpdateCarStock(id, stockLevel, dealerId);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCars([FromQuery] string make, [FromQuery] string model)
        {
            if (string.IsNullOrWhiteSpace(make) && string.IsNullOrWhiteSpace(model))
                return BadRequest("At least one of 'make' or 'model' must be provided.");

            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var cars = await _carRepository.SearchCars(make, model, dealerId);
            return Ok(cars);
        }
    }
}
