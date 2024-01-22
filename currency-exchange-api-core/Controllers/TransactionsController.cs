using currency_exchange_api_core.DTOs;
using currency_exchange_api_core.Enums;
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
    public class TransactionsController : ControllerBase
    {
        private readonly CurrencyExchangeApiDbContext _context;
        private readonly NbpService _nbpService;

        public TransactionsController(CurrencyExchangeApiDbContext context, NbpService nbpService)
        {
            _context = context;
            _nbpService = nbpService;
        }

        // POST: api/Transactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction([FromBody] TransactionDTO transactionData)
        {
            // Get the user ID from the token
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            if (transactionData == null)
            {
                return BadRequest();
            }

            if (transactionData.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            Wallet? fromWallet = null;
            Wallet? toWallet = null;

            if (transactionData.Type == TransactionTypes.Withdrawal || transactionData.Type == TransactionTypes.Transfer)
            {
                if (transactionData.FromWalletId == null)
                {
                    return BadRequest("FromWalletId must be provided.");
                }

                fromWallet = await _context.Wallets
                            .Include(w => w.TransactionWalletTos)
                                .ThenInclude(t => t.Conversions)
                            .Include(w => w.TransactionWalletFroms)
                                .ThenInclude(t => t.Conversions)
                            .FirstOrDefaultAsync(w => w.Id == transactionData.FromWalletId);
            }

            if (transactionData.Type == TransactionTypes.Deposit || transactionData.Type == TransactionTypes.Transfer)
            {
                if (transactionData.ToWalletId == null)
                {
                    return BadRequest("ToWalletId must be provided.");
                }

                toWallet = await _context.Wallets
                            .Include(w => w.TransactionWalletTos)
                                .ThenInclude(t => t.Conversions)
                            .Include(w => w.TransactionWalletFroms)
                                .ThenInclude(t => t.Conversions)
                            .FirstOrDefaultAsync(w => w.Id == transactionData.ToWalletId);
            }

            if (transactionData.Type == TransactionTypes.Deposit || transactionData.Type == TransactionTypes.Transfer)
            {
                if (toWallet == null)
                {
                    return NotFound($"wallet not found {transactionData.ToWalletId}");
                }

                if (toWallet.UserId != userId)
                {
                    return Unauthorized();
                }
            }

            if (transactionData.Type == TransactionTypes.Withdrawal || transactionData.Type == TransactionTypes.Transfer)
            {
                if (fromWallet == null)
                {
                    return NotFound($"wallet not found {transactionData.FromWalletId}");
                }

                if (fromWallet != null && fromWallet.UserId != userId)
                {
                    return Unauthorized();
                }
            }

            switch (transactionData.Type)
            {
                case TransactionTypes.Deposit:
                    {
                        if (fromWallet != null && toWallet == null)
                        {
                            return BadRequest("FromWalletId must be null and ToWalletId must be provided for deposit.");
                        }

                        break;
                    }
                case TransactionTypes.Withdrawal:
                    {
                        if (fromWallet == null && toWallet != null)
                        {
                            return BadRequest("FromWalletId must be provided and ToWalletId must be null for withdrawal.");
                        }

                        break;
                    }
                case TransactionTypes.Transfer:
                    {
                        if (fromWallet == null || toWallet == null)
                        {
                            return BadRequest("FromWalletId and ToWalletId must be provided for transfer.");
                        }

                        break;
                    }
                default:
                    {
                        return BadRequest("Invalid transaction type.");
                    }
            }

            var transaction = new Transaction
            {
                Type = transactionData.Type,
                WalletFromId = fromWallet?.Id,
                WalletToId = toWallet?.Id,
                Date = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            switch (transactionData.Type)
            {
                case TransactionTypes.Deposit:
                    {
                        var conversion = new Conversion
                        {
                            Order = 0,
                            CurrencyAfter = toWallet.Currency,
                            AmountAfter = transactionData.Amount,
                            TransactionId = transaction.Id
                        };

                        _context.Conversions.Add(conversion);
                        await _context.SaveChangesAsync();

                        break;
                    }
                case TransactionTypes.Withdrawal:
                    {
                        var conversion = new Conversion
                        {
                            Order = 0,
                            CurrencyBefore = fromWallet.Currency,
                            AmountBefore = transactionData.Amount,
                            TransactionId = transaction.Id
                        };

                        var currentBalance = CalculateCurrentWalletBalance(fromWallet);
                        var transactionValue = conversion.AmountBefore;

                        if (transactionValue > currentBalance)
                        {
                            return BadRequest("FromWallet balance is too low.");
                        }

                        _context.Conversions.Add(conversion);
                        await _context.SaveChangesAsync();

                        break;
                    }
                case TransactionTypes.Transfer:
                    {
                        //TODO: handle case when both currencies are the same

                        List<Conversion> conversions = new List<Conversion>();

                        if (fromWallet.Currency == "PLN" && toWallet.Currency != "PLN")
                        {
                            var rateTable = await _nbpService.GetCurrentCurrencyRate(toWallet.Currency);

                            if (rateTable == null)
                            {
                                return NotFound($"Current {toWallet.Currency} rate not available");
                            }

                            var rate = rateTable.rates[0].ask;

                            var conversion = new Conversion
                            {
                                Order = 0,
                                CurrencyBefore = "PLN",
                                AmountBefore = transactionData.Amount * rate,
                                CurrencyAfter = toWallet.Currency,
                                AmountAfter = transactionData.Amount,
                                TransactionId = transaction.Id,
                                Rate = rate
                            };

                            conversions.Add(conversion);
                        }
                        else if (fromWallet.Currency != "PLN" && toWallet.Currency == "PLN")
                        {
                            var rateTable = await _nbpService.GetCurrentCurrencyRate(fromWallet.Currency);

                            if (rateTable == null)
                            {
                                return NotFound($"Current {fromWallet.Currency} rate not available");
                            }

                            var rate = 1 / rateTable.rates[0].bid;

                            var conversion = new Conversion
                            {
                                Order = 0,
                                CurrencyBefore = fromWallet.Currency,
                                AmountBefore = transactionData.Amount * rate,
                                CurrencyAfter = "PLN",
                                AmountAfter = transactionData.Amount,
                                TransactionId = transaction.Id,
                                Rate = rate
                            };

                            conversions.Add(conversion);
                        }
                        else if (fromWallet.Currency != "PLN" && toWallet.Currency != "PLN")
                        {
                            var rateTableFrom = await _nbpService.GetCurrentCurrencyRate(fromWallet.Currency);
                            var rateTableTo = await _nbpService.GetCurrentCurrencyRate(toWallet.Currency);

                            if (rateTableFrom == null)
                            {
                                return NotFound($"Current {fromWallet.Currency} rate not available");
                            }

                            if (rateTableTo == null)
                            {
                                return NotFound($"Current {toWallet.Currency} rate not available");
                            }

                            var rateFrom = 1 / rateTableFrom.rates[0].bid;
                            var rateTo = rateTableTo.rates[0].ask;

                            var conversion1 = new Conversion
                            {
                                Order = 0,
                                CurrencyBefore = fromWallet.Currency,
                                AmountBefore = transactionData.Amount * rateTo * rateFrom,
                                CurrencyAfter = "PLN",
                                AmountAfter = transactionData.Amount * rateTo,
                                TransactionId = transaction.Id,
                                Rate = rateFrom
                            };

                            var conversion2 = new Conversion
                            {
                                Order = 1,
                                CurrencyBefore = "PLN",
                                AmountBefore = conversion1.AmountAfter,
                                CurrencyAfter = toWallet.Currency,
                                AmountAfter = transactionData.Amount,
                                TransactionId = transaction.Id,
                                Rate = rateTo
                            };

                            conversions.Add(conversion1);
                            conversions.Add(conversion2);
                        }
                        else
                        {
                            return BadRequest("Unhandled currency conversions");
                        }

                        var currentBalance = CalculateCurrentWalletBalance(fromWallet);
                        var transactionValue = conversions[0].AmountBefore;

                        if (transactionValue > currentBalance)
                        {
                            return BadRequest("FromWallet balance is too low.");
                        }

                        _context.Conversions.AddRange(conversions);
                        await _context.SaveChangesAsync();

                        break;
                    }
                default:
                    {
                        return BadRequest("Invalid transaction type.");
                    }
            }

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        private decimal CalculateCurrentWalletBalance(Wallet wallet)
        {
            decimal currentBalance = 0.0m;

            foreach (var transaction in wallet.TransactionWalletTos)
            {
                var lastConversion = transaction.Conversions.OrderBy(x => x.Order).LastOrDefault();

                if (lastConversion != null && lastConversion.AmountAfter != null)
                    currentBalance += lastConversion.AmountAfter.Value;
            }

            foreach (var transaction in wallet.TransactionWalletFroms)
            {
                var firstConversion = transaction.Conversions.OrderBy(x => x.Order).FirstOrDefault();

                if (firstConversion != null && firstConversion.AmountBefore != null)
                    currentBalance -= firstConversion.AmountBefore.Value;
            }

            return currentBalance;
        }
    }
}