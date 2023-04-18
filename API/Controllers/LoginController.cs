using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IntellicareContext _context;

        public LoginController(IntellicareContext context)
        {
            _context = context;
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

            return Ok(new { status = 200, isSuccess = true, message = "User Login successfully", UserDetails = user });
        }

        private string HashPassword(string? password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
            // Use a secure hashing algorithm to hash the password before storing it in the database
            // Example: return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
