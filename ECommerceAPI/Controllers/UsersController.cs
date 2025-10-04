using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceAPI.Dtos;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Identity;
namespace ECommerceAPI.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IConfiguration configuration;

        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var exists = await context.Users.AnyAsync(u => u.Email == user.Email);
            if (exists) return BadRequest("Email already exists.");
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return Ok(new { user.Id, user.Email });
        }
        //  Login and Get JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");
            var token = GenerateJwtToken(user);
            return Ok(new { token, user.Id, user.Name, user.Role });
        }

        // JWT Token Generator
        private string GenerateJwtToken(User user)
        {
            var jwtConfig = configuration.GetSection("jwt");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpireMinutes"])
                ),
                signingCredentials: creds
                );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);

        }

        //Get All Users (only for Admin)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await context.Users.ToListAsync();
            return Ok(user);
        }
        // Get Single User
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        //  Update User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser (int id, [FromBody] User updatedUser)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.PasswordHash = updatedUser.PasswordHash;
            user.Role = updatedUser.Role;

            await context.SaveChangesAsync();
            return Ok(user);
        }


        // Delete User
        [HttpDelete]
        public async Task <IActionResult> DeleteUser (int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null) return NotFound();
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return Ok(user);
        }
    }
}
