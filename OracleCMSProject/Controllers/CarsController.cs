using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var car = await _carRepository.GetCarById(id, dealerId);
            if (car == null) return NotFound("Car not found.");
            return Ok(car);
        }

        [HttpPost]
        public async Task<IActionResult> AddCar([FromBody] Car car)
        {
            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            car.DealerId = dealerId;     // Assign dealerId to the car
            
            await _carRepository.AddCar(car);
            return CreatedAtAction(nameof(GetCarById), new { id = car.Id }, car);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            int dealerId = GetDealerId(); // Retrieve dealerId from claims

            // Ensure the car belongs to the current dealer
            var existingCar = await _carRepository.GetCarById(id, dealerId);
            if (existingCar == null)
            {
                return NotFound("Car not found or you are not authorized to update this car.");
            }

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
            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var existingCar = await _carRepository.GetCarById(id, dealerId);
            if (existingCar == null)
            {
                return NotFound("Car not found or you are not authorized to Delete this car.");
            }
            await _carRepository.DeleteCar(id, dealerId);
            return NoContent();
        }
        
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateCarStock(int id, [FromBody] int stockLevel)
        {
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

        // Missing Function: SearchCars
        [HttpGet("search")]
        public async Task<IActionResult> SearchCars([FromQuery] string make, [FromQuery] string model)
        {
            int dealerId = GetDealerId(); // Retrieve dealerId from claims
            var cars = await _carRepository.SearchCars(make, model, dealerId);
            return Ok(cars);
        }

    }
}
