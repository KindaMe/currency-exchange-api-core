using currency_exchange_api_core.DTOs;
using currency_exchange_api_core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace currency_exchange_api_core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly CurrencyExchangeApiDbContext _context;

        public TokenController(CurrencyExchangeApiDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody] UserCredentialsDTO userCredentials)
        {
            if (userCredentials == null || string.IsNullOrEmpty(userCredentials.Email) || string.IsNullOrEmpty(userCredentials.Password))
            {
                return BadRequest("Invalid credentials");
            }

            var matchingUser = _context.Users.FirstOrDefault(x => x.Email == userCredentials.Email && x.Password == userCredentials.Password);

            if (matchingUser == null)
            {
                return Unauthorized();
            }

            // Create JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("rD5gf5QHTqTWGWxBC6PNhRnRYnnTdib8"); // Replace with your secret key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserId", matchingUser.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Return token to the client
            return Ok(new { Token = tokenString });
        }

        //[HttpGet]
        //public IActionResult Validate()
        //{
        //    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        return BadRequest("Invalid token");
        //    }

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes("rD5gf5QHTqTWGWxBC6PNhRnRYnnTdib8");
        //    var validationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(key),
        //        ValidateLifetime = true,
        //        ValidateAudience = false,
        //        ValidateIssuer = false,
        //        ClockSkew = TimeSpan.Zero
        //    };

        //    SecurityToken securityToken;
        //    var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

        //    var jwtSecurityToken = securityToken as JwtSecurityToken;

        //    if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        return BadRequest("Invalid token");
        //    }

        //    // Token is valid so we can return it
        //    return Ok(new { Token = token });
        //}

        //[HttpGet]
        //public IActionResult Check()
        //{
        //    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes("rD5gf5QHTqTWGWxBC6PNhRnRYnnTdib8"); // Replace with your secret key
        //    try
        //    {
        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateLifetime = true, // Check if the token is not expired
        //            ValidateAudience = false,
        //            ValidateIssuer = false,
        //            ClockSkew = TimeSpan.Zero
        //        }, out SecurityToken validatedToken);
        //    }
        //    catch
        //    {
        //        return Unauthorized();
        //    }

        //    return Ok();
        //}
    }
}