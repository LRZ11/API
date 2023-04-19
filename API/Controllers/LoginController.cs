using API.Authentication;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IntellicareContext _context;
        public IConfiguration _configuration;

        public LoginController(IConfiguration configuration, IntellicareContext context)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] Login value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.UserLogins.SingleOrDefault(u => u.Username == value.username);
            
            if (user == null)
            {
                //return BadRequest("Invalid Username or Password");
                return Ok(new { status = 401, isSuccess = false, message = "Invalid Username or Password", });
            }
            var hashpassword = HashPassword(value.password);

            if (user.Password != value.password)
            {
                //return BadRequest("Invalid Username or Password");
                return Ok(new { status = 401, isSuccess = false, message = "Incorrect Username or Password", });
            }

            var jwtauth = new JwtAuthManager(_configuration);
            var token = jwtauth.GenerateJwtToken(user);

            return Ok(token);

            //return Ok(new { status = 200, isSuccess = true, token = finalToken, message = "User Login successfully", UserDetails = user });
            //return Ok(finalToken);
        }

        private async Task<UserLogin> GetUser(string username, string password)
        {

            return await _context.UserLogins.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
            // Use a secure hashing algorithm to hash the password before storing it in the database
            // Example: return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
