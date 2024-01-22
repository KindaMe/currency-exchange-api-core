using currency_exchange_api_core.DTOs;
using currency_exchange_api_core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace currency_exchange_api_core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Regex _emailRegex = new Regex(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");

        private readonly CurrencyExchangeApiDbContext _context;

        public UsersController(CurrencyExchangeApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetUserDetails()
        {
            // Get the user ID from the token
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            // Retrieve wallets based on the user ID
            var user = await _context.Users.FindAsync(userId);

            if (user != null && user.IsActive == 1)
            {
                user.Password = new string('*', user.Password.Length);
                return user;
            }
            else
            {
                return NotFound();
            }
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.Email = user.Email.Trim();
            user.Password = user.Password.Trim();
            user.FirstName = user.FirstName.Trim();
            user.LastName = user.LastName.Trim();

            if (!_emailRegex.IsMatch(user.Email))
            {
                return BadRequest(new { message = "Email is not in correct format" });
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
            {
                return Conflict("Email already in use");
            }

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            _context.Wallets.Add(new Wallet
            {
                Currency = "PLN",
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            return Created();
        }

        // PUT: api/Users
        [Authorize]
        [HttpPut("Email")]
        public async Task<IActionResult> PutEmail([FromBody] UserUpdatedDetailsDTO userDetails)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            if (userDetails.NewEmail == null)
            {
                return BadRequest("Email needs to be provided");
            }

            var trimmedEmail = userDetails.NewEmail.Trim();

            if (!_emailRegex.IsMatch(trimmedEmail))
            {
                return BadRequest(new { message = "Email is not in correct format" });
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == trimmedEmail);
            if (emailExists)
            {
                return Conflict("Email already in use");
            }

            var userToUpdate = await _context.Users.FindAsync(userId);

            if (userToUpdate == null || userToUpdate.IsActive != 1)
            {
                return NotFound("User not found");
            }

            if (userToUpdate.Password != userDetails.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            if (userToUpdate.Email == trimmedEmail)
            {
                return BadRequest("New and old emails are the same");
            }

            userToUpdate.Email = trimmedEmail;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Users
        [Authorize]
        [HttpPut("Password")]
        public async Task<IActionResult> PutPassword([FromBody] UserUpdatedDetailsDTO userDetails)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            if (userDetails.NewPassword == null)
            {
                return BadRequest("New password must be provided");
            }

            var trimmedPassword = userDetails.NewPassword.Trim();

            var userToUpdate = await _context.Users.FindAsync(userId);

            if (userToUpdate == null || userToUpdate.IsActive != 1)
            {
                return NotFound("User not found");
            }

            if (userToUpdate.Password != userDetails.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            if (userToUpdate.Password == trimmedPassword)
            {
                return BadRequest("New and old passwords are the same");
            }

            userToUpdate.Password = trimmedPassword;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] UserUpdatedDetailsDTO userDetails)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsActive != 1)
            {
                return NotFound("User not found");
            }

            if (user.Password != userDetails.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            user.IsActive = 0;
            user.FirstName = "deleted";
            user.LastName = "deleted";
            user.Email = "deleted";
            user.Password = "deleted";

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}