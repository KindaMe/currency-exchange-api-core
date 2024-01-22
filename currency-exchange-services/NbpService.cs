using currency_exchange_api_core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace currency_exchange_services
{
    public interface INbpService
    {
        void MidRatesConverter(NbpTable table);
    }

    public class NbpService : INbpService
    {
        private readonly CurrencyExchangeApiDbContext _context;

        public NbpService(CurrencyExchangeApiDbContext context)
        {
            _context = context;
        }

        public void MidRatesConverter(NbpTable table)
        {
            if (table.rates != null && _context != null)
            {
                foreach (var rate in table.rates)
                {
                    var rateDate = !string.IsNullOrEmpty(rate.effectiveDate) ? DateTime.Parse(rate.effectiveDate) : DateTime.Parse(table.effectiveDate);

                    var cut = _context.Cuts
                        .Where(c => c.EffectiveDate <= rateDate)
                        .OrderByDescending(c => c.EffectiveDate)
                        .FirstOrDefault();

                    if (cut != null)
                    {
                        rate.bid = Math.Round(rate.mid + (rate.mid * (cut.BuyPercentage / 100)), 8);
                        rate.ask = Math.Round(rate.mid - (rate.mid * (cut.SellPercentage / 100)), 8);
                    }
                    else
                    {
                        //no cut found
                    }
                }
            }
        }
    }
}