using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using dex_webapp.Data;
using dex_webapp.Models;
using dex_webapp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace dex_webapp.Services
{
    public interface ITradesService
    {
        Task<IEnumerable<TradeViewModel>> GetTrades(string token);
        Task<IEnumerable<TradeViewModel>> GetMyTrades(string token, string address);
        Task<IEnumerable<MarketViewModel>> GetMarkets();
    }
    public class TradesService : ITradesService
    {
        private const string NullAddress = "0x0000000000000000000000000000000000000000";
        private readonly ApplicationDbContext _context;
        private readonly Web3 _nethereumClient;

        public TradesService(ApplicationDbContext context, IOptions<EthereumSettings> ethSettings)
        {
            _context = context;
            _nethereumClient = new Web3(ethSettings.Value.EthereumRpcNodeUrl);

        }

        public async Task<IEnumerable<TradeViewModel>> GetTrades(string tokenSymbol)
        {
            var token = _context.Token.FirstOrDefault(x => x.Symbol == tokenSymbol) ?? null;
            if (token == null) throw new Exception("Token contact not found");

            var query = from trade in _context.TradeEvent
                        where trade.TokenGet == token.Token || trade.TokenGive == token.Token
                        orderby trade.BlockNum descending
                        select new TradeViewModel()
                        {
                            TokenGet = trade.TokenGet,
                            AmountGet = trade.AmountGet,
                            TokenGive = trade.TokenGive,
                            AmountGive = trade.AmountGive,
                            Timestamp = trade.Timestamp
                        };
            return await query.Take(100).ToListAsync();
        }

        public async Task<IEnumerable<TradeViewModel>> GetMyTrades(string tokenSymbol, string address)
        {
            var token = _context.Token.FirstOrDefault(x => x.Symbol == tokenSymbol) ?? null;
            if (token == null) throw new Exception("Token contact not found");

            var query = from trade in _context.TradeEvent
                        where (trade.TokenGet == token.Token || trade.TokenGive == token.Token) && (string.Equals(trade.Get, address, StringComparison.InvariantCultureIgnoreCase) || string.Equals(trade.Give, address, StringComparison.InvariantCultureIgnoreCase))
                        orderby trade.BlockNum descending
                        select new TradeViewModel()
                        {
                            TokenGet = trade.TokenGet,
                            AmountGet = trade.AmountGet,
                            TokenGive = trade.TokenGive,
                            AmountGive = trade.AmountGive,
                            Timestamp = trade.Timestamp
                        };
            return await query.Take(100).ToListAsync();
        }

        public async Task<IEnumerable<MarketViewModel>> GetMarkets()
        {
            var tokens = await _context.Token.Where(x => x.Status > 0).ToListAsync(); //
            var trades = await _context.TradeEvent
                .Where(_ => _.Timestamp.HasValue && _.Timestamp.Value >= DateTimeOffset.UtcNow.AddDays(-1))
                .OrderBy(_ => _.Timestamp.Value)
                .ToListAsync();

            List<MarketViewModel> results = new List<MarketViewModel>();
            var tokenTrades = trades.GroupBy(_ => _.TokenGet == NullAddress ? _.TokenGive : _.TokenGet);
            foreach (var tokenTradeList in tokenTrades)
            {
                var marketModel = new MarketViewModel();
                if (tokens.All(_ => _.Token != tokenTradeList.Key)) continue;
                var token = tokens.FirstOrDefault(_ => _.Token == tokenTradeList.Key);
                tokens.Remove(token);


                marketModel.Symbol = token?.Symbol ?? "unknown";
                BigInteger volume = 0;
                foreach (var trade in tokenTradeList)
                {
                    string amount = trade.TokenGet == NullAddress ? trade.AmountGive : trade.AmountGet;
                    volume += BigInteger.Parse(amount);
                }
                marketModel.Volume = volume.ToString();
                if (tokenTradeList.Count() > 0)
                {
                    decimal startPrice = GetTradePrice(tokenTradeList.First());
                    decimal endPrice = GetTradePrice(tokenTradeList.Last());
                    marketModel.Price = endPrice;
                    if (startPrice != 0)
                        marketModel.Change = endPrice / startPrice - 1;
                }
                results.Add(marketModel);
            }

            foreach (var token in tokens) // tokens without trades
            {
                results.Add(new MarketViewModel
                {
                    Symbol = token.Symbol,
                    Price = 0,
                    Volume = "0",
                    Change = 0
                });
            }
            return results;
        }

        public decimal GetTradePrice(TradeEventModel trade)
        {
            var amountGet = Convert.ToDecimal(trade.AmountGet);
            var amountGive = Convert.ToDecimal(trade.AmountGive);
            if (amountGet == 0 || amountGive == 0)
                return 0;
            if (trade.TokenGet == NullAddress)
                return amountGive / amountGet;
            else
                return amountGet / amountGive;
        }
    }
}
