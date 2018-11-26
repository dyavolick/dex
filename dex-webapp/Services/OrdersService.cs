using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using dex_webapp.Data;
using dex_webapp.Models;
using dex_webapp.Models.ViewModels;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace dex_webapp.Services
{
    public interface IOrdersService
    {
        Task<IEnumerable<OrderViewModel>> GetOrders(string token, string address);
        Task<IEnumerable<OrderViewModel>> GetOrders(string token);
    }
    public class OrdersService : IOrdersService
    {
        private const string NullAddress = "0x0000000000000000000000000000000000000000";
        private readonly AbiProvider _abiProvider;
        private readonly Web3 _nethereumClient;
        private readonly EthereumSettings _ethSettings;
        private readonly ApplicationDbContext _context;

        public OrdersService(AbiProvider abiProvider, IOptions<EthereumSettings> ethSettings, ApplicationDbContext context)
        {
            _abiProvider = abiProvider;
            _nethereumClient = new Web3(ethSettings.Value.EthereumRpcNodeUrl);
            _context = context;
            _ethSettings = ethSettings.Value;
        }

        public async Task<IEnumerable<OrderViewModel>> GetOrders(string tokenSymbol, string address)
        {
            //var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            //var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            //return null;

            var lastBlock = (await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value.ToString();
            var token = _context.Token.FirstOrDefault(x => x.Symbol == tokenSymbol);
            if (token == null) throw new Exception("token not found");
            var query = from order in _context.OrderEvent
                        join filled in _context.OrderFilled on order.Hash equals filled.Hash into joinedC
                        from filled in joinedC.DefaultIfEmpty()
                        where order.User == address.ToLowerInvariant() && order.Expires.CompareTo(lastBlock) > 0 && (order.TokenGet == token.Token || order.TokenGive == token.Token)
                        && (filled == null || !filled.IsDone) && _context.CancelEvent.All(x => order.Hash != x.Hash)
                        select new OrderViewModel()
                        {
                            Id = order.Id,
                            TokenGet = order.TokenGet,
                            AmountGet = order.AmountGet,
                            TokenGive = order.TokenGive,
                            AmountGive = order.AmountGive,
                            Expires = order.Expires,
                            Nonce = order.Nonce,
                            Available = filled == null ? "" : filled.AmountAvailable,
                            Filled = filled == null ? "" : filled.AmountFilled,
                            User = order.User,
                            Hash = order.Hash
                        };
            return query.ToList();
        }

        public async Task<IEnumerable<OrderViewModel>> GetOrders(string tokenSymbol)
        {
            var lastBlock = (await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value.ToString();
            var token = _context.Token.FirstOrDefault(x => x.Symbol == tokenSymbol);
            if (token == null) throw new Exception("token not found");
            try
            {
                var query = from order in _context.OrderEvent
                            join filled in _context.OrderFilled on order.Hash equals filled.Hash into joinedC
                            from filled in joinedC.DefaultIfEmpty()
                            where order.Expires.CompareTo(lastBlock) > 0 && (order.TokenGet == token.Token || order.TokenGive == token.Token)
                                  && _context.CancelEvent.All(x => order.Hash != x.Hash) && (filled == null || !filled.IsDone)
                            select new OrderViewModel()
                            {
                                Id = order.Id,
                                TokenGet = order.TokenGet,
                                AmountGet = order.AmountGet,
                                TokenGive = order.TokenGive,
                                AmountGive = order.AmountGive,
                                Expires = order.Expires,
                                Nonce = order.Nonce,
                                Available = filled == null ? "" : filled.AmountAvailable,
                                Filled = filled == null ? "" : filled.AmountFilled,
                                User = order.User,
                                Hash = order.Hash
                            };
                return query.ToList();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
