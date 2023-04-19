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
        private readonly JwtSettings jwtSettings;
        public IConfiguration _configuration;

        public LoginController(IConfiguration configuration, IOptions<JwtSettings> option, IntellicareContext context)
        {
            _context = context;
            this.jwtSettings = option.Value;
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

            var tokenhandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value);
            //var claims = new[] {
            //                    //new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
            //                    //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //                    //new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            //                    new Claim("UserId", user.Id.ToString()),
            //                    new Claim("DisplayName", user.Lastname),
            //                    new Claim("UserName", user.Username),
            //                    //new Claim("Email", user.Email)
            //                };
            var claim = new Claim[] {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                //new Claim(ClaimTypes.Name, user.Lastname)
            };

            var tokendesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claim),

                //Subject = new ClaimsIdentity(claim),
                Expires = DateTime.Now.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                    
            };

            var token = tokenhandler.CreateToken(tokendesc);
            string finalToken = tokenhandler.WriteToken(token);


            return Ok(new { status = 200, isSuccess = true, token = finalToken, message = "User Login successfully", UserDetails = user });
            //return Ok(finalToken);
        }

        //[HttpPost]
        //[Route("Auth")]
        //public async Task<IActionResult> AuthToken(Login? userLog)
        //{
        //    if (userLog != null && userLog.username != null && userLog.password != null)
        //    {
        //        UserLogin? user = await GetUser(userLog.username, userLog.password);

        //        if (user != null)
        //        {
        //            //create claims details based on the user information
        //            var claims = new[] {
        //                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
        //                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
        //                new Claim("UserId", user.Id.ToString()),
        //                new Claim("DisplayName", user.Lastname),
        //                new Claim("UserName", user.Username),
        //                //new Claim("Email", user.Email)
        //            };

        //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //            var token = new JwtSecurityToken(
        //                _configuration["Jwt:Issuer"],
        //                _configuration["Jwt:Audience"],
        //                claims,
        //                expires: DateTime.UtcNow.AddMinutes(10),
        //                signingCredentials: signIn);

        //            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        //        }
        //        else
        //        {
        //            return BadRequest("Invalid credentials");
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}
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
