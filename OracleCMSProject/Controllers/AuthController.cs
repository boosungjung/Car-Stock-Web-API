
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OracleCMSProject.Models;
using System.Data;

namespace OracleCMSProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDbConnection _connection;
        private readonly IConfiguration _configuration;

        public AuthController(IDbConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }
        
        /// <summary>
        ///  Register a new user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            // Check if username already exists
            var existingUser = await _connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username = @Username",
                new { request.Username }
            );

            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            // Hash the password and save the user
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            // Insert the new user
            await _connection.ExecuteAsync(
                "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash);",
                user
            );

            return Ok(user);
        }
        
        /// <summary>
        ///  Login a user and return a JWT token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            // Check if user exists
            var user = await _connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Username = @Username",
                new { request.Username }
            );

            if (user == null)
            {
                return Unauthorized("Username or Password is Invalid.");
            }

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Username or Password is Invalid.");
            }

            string token = CreateToken(user);
            return Ok(new { token });
        }
        
        /// <summary>
        ///  Create a JWT token for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!
            ));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenHandler;
        }
    }
}
