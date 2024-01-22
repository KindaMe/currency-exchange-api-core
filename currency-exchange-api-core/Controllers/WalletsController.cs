using currency_exchange_api_core.DTOs;
using currency_exchange_api_core.Models;
using currency_exchange_api_core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace currency_exchange_api_core.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private readonly CurrencyExchangeApiDbContext _context;
        private readonly NbpService _nbpService;
        private readonly CountriesService _countriesService;

        public WalletsController(CurrencyExchangeApiDbContext context, NbpService nbpService, CountriesService countriesService)
        {
            _context = context;
            _nbpService = nbpService;
            _countriesService = countriesService;
        }

        // GET: api/Wallets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wallet>>> GetWallets()
        {
            // Get the user ID from the token
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            // Retrieve wallets based on the user ID
            var wallets = await _context.Wallets
                .Include(w => w.TransactionWalletTos)
                    .ThenInclude(t => t.Conversions)
                .Include(w => w.TransactionWalletTos)
                .Include(w => w.TransactionWalletFroms)
                    .ThenInclude(t => t.Conversions)
                .Include(w => w.TransactionWalletFroms)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            var tasks = wallets.Select(async wallet => wallet.FlagUrl = await _countriesService.GetFlagUrl(wallet.Currency));
            await Task.WhenAll(tasks);

            return wallets;
        }

        // POST: api/Wallets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Wallet>> PostWallet([FromBody] WalletDto data)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var exists = _context.Wallets.Any(w => w.Currency == data.Code && w.UserId == userId);

            if (exists)
            {
                return Conflict();
            }

            var wallet = new Wallet
            {
                Currency = data.Code,
                UserId = userId
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWallet", new { id = wallet.Id }, wallet);
        }

        // GET: api/Wallets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Wallet>> GetWallet(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }

        //// DELETE: api/Wallets/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteWallet(int id)
        //{
        //    var wallet = await _context.Wallets.FindAsync(id);
        //    if (wallet == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Wallets.Remove(wallet);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
    }
}