using dex_webapp.Data;
using dex_webapp.Models;
using dex_webapp.Models.Ethereum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using dex_webapp.Models.ViewModels;
using dex_webapp.Services.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Nethereum.StandardTokenEIP20;
using Nethereum.StandardTokenEIP20.CQS;
using Nethereum.Util;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace dex_webapp.Services
{
    public interface IEthereumService
    {
        Task ScanOrdersByEventsAsync();

        Task ScanCancelByEventsAsync();

        Task ScanDepositByEventsAsync();

        Task ScanTradeByEventsAsync();

        Task ScanWithdrawByEventsAsync();

        Task ScanActivateTokenByEventsAsync();

        Task ScanDeactivateTokenByEventsAsync();
        Task ScanOrdersFilledAsync();
    }

    public class EthereumService : IEthereumService
    {
        private readonly ApplicationDbContext _context;
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly ParametersService _parametersService;
        private readonly EtherscanService _etherscanService;
        private readonly ILogger<EthereumService> _logger;
        private readonly AbiProvider _abiProvider;
        private readonly Web3 _nethereumClient;
        private readonly EthereumSettings _ethSettings;
        private readonly IOHCLService _ohclService;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<OrderSignalR> _ordersHubContext;

        private const string NullAddress = "0x0000000000000000000000000000000000000000";

        public EthereumService(ApplicationDbContext context,
            IOptions<EthereumSettings> ethSettings,
            DbContextOptions<ApplicationDbContext> contextOptions,
            ParametersService parametersService,
               EtherscanService etherscanService,
            ILogger<EthereumService> logger,
            AbiProvider abiProvider, IOHCLService ohclService, IMemoryCache cache, IHubContext<OrderSignalR> ordersHubContext)
        {
            _context = context;
            _contextOptions = contextOptions;
            _parametersService = parametersService;
            _etherscanService = etherscanService;
            _logger = logger;
            _abiProvider = abiProvider;
            _ohclService = ohclService;
            _cache = cache;
            _ordersHubContext = ordersHubContext;
            _nethereumClient = new Web3(ethSettings.Value.EthereumRpcNodeUrl);
            _ethSettings = ethSettings.Value;
        }

        public async Task ScanCancelByEventsAsync()
        {
            string latestScannedBlockKey = "CancelLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var cancelEvent = contract.GetEvent("Cancel");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());


            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = cancelEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = cancelEvent.DecodeAllEventsForEvent<CancelEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.CancelEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new CancelEventModel
                {
                    TokenGet = eventEntry.Event.TokenGet,
                    AmountGet = eventEntry.Event.AmountGet.ToString(),
                    TokenGive = eventEntry.Event.TokenGive,
                    AmountGive = eventEntry.Event.AmountGive.ToString(),
                    Expires = eventEntry.Event.Expires.ToString(),
                    Nonce = eventEntry.Event.Nonce.ToString(),
                    User = eventEntry.Event.User,

                    Hash = ComputeHash(eventEntry.Event as CancelEvent),
                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.CancelEvent.AddAsync(dbEvent);
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanDepositByEventsAsync()
        {
            string latestScannedBlockKey = "DepositLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var depositEvent = contract.GetEvent("Deposit");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = depositEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = depositEvent.DecodeAllEventsForEvent<DepositEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.DepositEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new DepositEventModel
                {
                    Token = eventEntry.Event.Token,
                    User = eventEntry.Event.User,
                    Amount = eventEntry.Event.Amount.ToString(),
                    Balance = eventEntry.Event.Balance.ToString(),

                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.DepositEvent.AddAsync(dbEvent);
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanOrdersByEventsAsync()
        {
            string latestScannedBlockKey = "OrderLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var orderEvent = contract.GetEvent("Order");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = orderEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = orderEvent.DecodeAllEventsForEvent<OrderEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.OrderEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new OrderEventModel
                {
                    TokenGet = eventEntry.Event.TokenGet,
                    AmountGet = eventEntry.Event.AmountGet.ToString(),
                    TokenGive = eventEntry.Event.TokenGive,
                    AmountGive = eventEntry.Event.AmountGive.ToString(),
                    Expires = eventEntry.Event.Expires.ToString(),
                    Nonce = eventEntry.Event.Nonce.ToString(),
                    User = eventEntry.Event.User,

                    Hash = ComputeHash(eventEntry.Event as OrderEvent),
                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.OrderEvent.AddAsync(dbEvent);
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanTradeByEventsAsync()
        {
            string latestScannedBlockKey = "TradeLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var tradeEvent = contract.GetEvent("Trade");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = tradeEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = tradeEvent.DecodeAllEventsForEvent<TradeEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.TradeEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);
            var tokensToUpdate = new List<string>();
            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new TradeEventModel
                {
                    TokenGet = eventEntry.Event.TokenGet,
                    AmountGet = eventEntry.Event.AmountGet.ToString(),
                    TokenGive = eventEntry.Event.TokenGive,
                    AmountGive = eventEntry.Event.AmountGive.ToString(),
                    Get = eventEntry.Event.Get,
                    Give = eventEntry.Event.Give,

                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                tokensToUpdate.Add(await WriteOHLC(eventEntry.Event as TradeEvent, dbEvent.Timestamp));
                await _context.TradeEvent.AddAsync(dbEvent);
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);
            await _context.SaveChangesAsync();

            tokensToUpdate.ForEach(token => { UpdateChart(token).Wait(); });
        }

        private async Task<string> WriteOHLC(TradeEvent ev, DateTimeOffset? timestamp)
        {
            try
            {
                string tokenSymbol; 
                decimal tokenAmount, ethAmount, price = 0;
                TokenModel token;
                if (ev.TokenGet == NullAddress)
                {
                    token = await _context.Token.FirstOrDefaultAsync(x => x.Token == ev.TokenGive);
                    tokenSymbol = token.Symbol;
                    var d = await _cache.GetOrCreateAsync(ev.TokenGive, entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        var tokenContract = new StandardTokenService(_nethereumClient, ev.TokenGive);
                        var decimals = tokenContract.DecimalsQueryAsync(new DecimalsFunction()).Result;
                        return Task.FromResult((decimal)Math.Pow(10, decimals));
                    });
                    tokenAmount = (decimal)BigInteger.DivRem(ev.AmountGive, new BigInteger(d), out var remainder);
                    if (remainder > 0) tokenAmount += (decimal)remainder / d;

                    ethAmount = UnitConversion.Convert.FromWei(ev.AmountGet, UnitConversion.EthUnit.Ether);
                    if (tokenAmount > 0)
                    {
                        price = ethAmount / tokenAmount;
                    }
                }
                else
                {
                    token = await _context.Token.FirstOrDefaultAsync(x => x.Token == ev.TokenGet);
                    tokenSymbol = token.Symbol;
                    var d = await _cache.GetOrCreateAsync(ev.TokenGet, entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        var tokenContract = new StandardTokenService(_nethereumClient, ev.TokenGive);
                        var decimals = tokenContract.DecimalsQueryAsync(new DecimalsFunction()).Result;
                        return Task.FromResult((decimal)Math.Pow(10, decimals));
                    });
                    tokenAmount = (decimal)BigInteger.DivRem(ev.AmountGet, new BigInteger(d), out var remainder);
                    if (remainder > 0) tokenAmount += (decimal)remainder / d;

                    ethAmount = UnitConversion.Convert.FromWei(ev.AmountGive, UnitConversion.EthUnit.Ether);
                    if (tokenAmount > 0)
                    {
                        price = ethAmount / tokenAmount;
                    }
                }
                _ohclService.WriteOHLC(price, tokenAmount, token.Symbol, timestamp.Value.UtcDateTime);
                return tokenSymbol;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        public async Task ScanWithdrawByEventsAsync()
        {
            string latestScannedBlockKey = "WithdrawLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var withdrawEvent = contract.GetEvent("Withdraw");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = withdrawEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = withdrawEvent.DecodeAllEventsForEvent<WithdrawEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.WithdrawEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new WithdrawEventModel
                {
                    Token = eventEntry.Event.Token,
                    User = eventEntry.Event.User,
                    Amount = eventEntry.Event.Amount.ToString(),
                    Balance = eventEntry.Event.Balance.ToString(),

                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.WithdrawEvent.AddAsync(dbEvent);
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanActivateTokenByEventsAsync()
        {
            string latestScannedBlockKey = "ActivateTokenLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var tradeEvent = contract.GetEvent("ActivateToken");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = tradeEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = tradeEvent.DecodeAllEventsForEvent<ActivateTokenEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.ActivateTokenEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new ActivateTokenEventModel
                {
                    Token = eventEntry.Event.Token,
                    Symbol = eventEntry.Event.Symbol,

                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.ActivateTokenEvent.AddAsync(dbEvent);

                var token = await _context.Token.FirstOrDefaultAsync(t => t.Token == eventEntry.Event.Token);// ?? new TokenModel();
                if (token == null)
                {
                    token = new TokenModel
                    {
                        Token = dbEvent.Token,
                        Symbol = dbEvent.Symbol,
                        Status = 1,
                        StatusDateUpdate = DateTime.Now,
                        StatusBlockUpdate = dbEvent.BlockNum,
                    };
                    await _context.Token.AddAsync(token);
                }
                else if (token.StatusBlockUpdate < dbEvent.BlockNum)
                {
                    token.Token = dbEvent.Token;
                    token.Symbol = dbEvent.Symbol;
                    token.Status = 1;
                    token.StatusDateUpdate = DateTime.Now;
                    token.StatusBlockUpdate = dbEvent.BlockNum;
                    _context.Token.Update(token);
                }
                await _context.SaveChangesAsync();
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanDeactivateTokenByEventsAsync()
        {
            string latestScannedBlockKey = "DeactivateTokenLatestScannedBlock";
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            var withdrawEvent = contract.GetEvent("DeactivateToken");

            var blockNumHex = await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumLong = Convert.ToInt64(blockNumHex.Value.ToString());

            //to avoid races conditions between services
            blockNumLong -= 5;

            var latestScannedBlock = await _parametersService.GetLatestScannedBlockAsync(latestScannedBlockKey);
            var fromBlock = new BlockParameter(Convert.ToUInt64(latestScannedBlock + 1));

            var filterInput = withdrawEvent.CreateFilterInput(fromBlock);

            var logs = await _etherscanService.GetEventsAsync(filterInput, latestScannedBlock + 1, blockNumLong);

            var eventLogsAll = withdrawEvent.DecodeAllEventsForEvent<DeactivateTokenEvent>(logs);

            var detectedTransactionHashes = eventLogsAll.Select(l => l.Log.TransactionHash).ToList();

            var existingDbTransactions = _context.DeactivateTokenEvent
                .Where(t => detectedTransactionHashes.Contains(t.TransactionHash))
                .ToDictionary(t => t.TransactionHash, t => t);

            foreach (var eventEntry in eventLogsAll)
            {
                if (existingDbTransactions.TryGetValue(eventEntry.Log.TransactionHash, out var dbTransaction))
                {
                    continue;
                }

                var transaction = await _nethereumClient.Eth.Transactions.GetTransactionByHash.SendRequestAsync(eventEntry.Log.TransactionHash);
                var receipt = await _nethereumClient.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(eventEntry.Log.TransactionHash);
                var block = await _nethereumClient.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transaction.BlockNumber);

                var epoch = Convert.ToInt64(block.Timestamp.Value.ToString());

                var dbEvent = new DeactivateTokenEventModel
                {
                    Token = eventEntry.Event.Token,
                    Symbol = eventEntry.Event.Symbol,

                    TransactionHash = eventEntry.Log.TransactionHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch),
                    BlockNum = Convert.ToInt64(transaction.BlockNumber.Value.ToString()),
                    GasPriceWei = Convert.ToInt64(transaction.GasPrice.Value.ToString()),
                    GasUsed = Convert.ToInt64(receipt.GasUsed.Value.ToString()),
                };
                await _context.DeactivateTokenEvent.AddAsync(dbEvent);

                var token = await _context.Token.FirstOrDefaultAsync(t => t.Token == eventEntry.Event.Token);// ?? new TokenModel();
                if (token == null)
                {
                    token = new TokenModel
                    {
                        Token = dbEvent.Token,
                        Symbol = dbEvent.Symbol,
                        Status = 0,
                        StatusDateUpdate = DateTime.Now,
                        StatusBlockUpdate = dbEvent.BlockNum
                    };
                    await _context.Token.AddAsync(token);
                }
                else if (token.StatusBlockUpdate < dbEvent.BlockNum)
                {
                    token.Token = dbEvent.Token;
                    token.Symbol = dbEvent.Symbol;
                    token.Status = 0;
                    token.StatusDateUpdate = DateTime.Now;
                    token.StatusBlockUpdate = dbEvent.BlockNum;
                    _context.Token.Update(token);
                }
                await _context.SaveChangesAsync();
            }

            await _parametersService.SetLatestScannedBlockAsync(latestScannedBlockKey, blockNumLong);

            await _context.SaveChangesAsync();
        }

        public async Task ScanOrdersFilledAsync()
        {
            var lastBlock = (await _nethereumClient.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value.ToString();

            var query = from order in _context.OrderEvent
                        join filled in _context.OrderFilled on order.Hash equals filled.Hash into joinedC
                        from filled in joinedC.DefaultIfEmpty()
                        where order.Expires.CompareTo(lastBlock) > 0 && _context.CancelEvent.All(x => x.Hash != order.Hash) && (filled == null || !filled.IsDone) 
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
            var activeOrders = query.ToList();
            var abi = await _abiProvider.GetCrowdsaleAbiAsync();
            var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);
            foreach (var order in activeOrders)
            {
                try
                {
                    //var decimalsFunc = contract.GetFunction("admin");
                    //var decimals = await decimalsFunc.CallAsync<string>();

                    var func1 = contract.GetFunction("availableVolume");
                    var availableVolume = await func1.CallAsync<BigInteger>(order.TokenGet, BigInteger.Parse(order.AmountGet), order.TokenGive, BigInteger.Parse(order.AmountGive), BigInteger.Parse(order.Expires), BigInteger.Parse(order.Nonce), order.User);

                    var func = contract.GetFunction("amountFilled");
                    var amountFilled = await func.CallAsync<BigInteger>(order.TokenGet, BigInteger.Parse(order.AmountGet), order.TokenGive, BigInteger.Parse(order.AmountGive), BigInteger.Parse(order.Expires), BigInteger.Parse(order.Nonce), order.User);
                    var orderFilled = _context.OrderFilled.FirstOrDefault(x => x.Hash == order.Hash);
                    if (orderFilled == null)
                    {
                        orderFilled = new OrderFilledModel()
                        {
                            Hash = order.Hash,
                            IsDone = false
                        };
                        _context.OrderFilled.Add(orderFilled);
                    }
                    orderFilled.AmountFilled = amountFilled.ToString();
                    if (orderFilled.AmountFilled == order.AmountGet)
                    {
                        orderFilled.IsDone = true;
                    }
                    orderFilled.AmountAvailable = availableVolume.ToString();
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
             
            }
            //throw new NotImplementedException();
        }

        static string GetSha256Hash(SHA256 shaHash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private string ComputeHash(CancelEvent eventEntry)
        {
            return GetSha256Hash(SHA256.Create(), (eventEntry.TokenGet + eventEntry.AmountGet +
                                                   eventEntry.TokenGive + eventEntry.AmountGive +
                                                   eventEntry.Expires + eventEntry.Nonce +
                                                   eventEntry.User).ToLowerInvariant());
        }

        private string ComputeHash(OrderEvent eventEntry)
        {
            return GetSha256Hash(SHA256.Create(), (eventEntry.TokenGet + eventEntry.AmountGet +
                                                   eventEntry.TokenGive + eventEntry.AmountGive +
                                                   eventEntry.Expires + eventEntry.Nonce +
                                                   eventEntry.User).ToLowerInvariant());
        }

        private async Task UpdateChart(string currencyPairId)
        {
            var tradeLastCandles = await _ohclService.GetChartLastCandles(currencyPairId);
            foreach (MarketDataItemRange range in Enum.GetValues(typeof(MarketDataItemRange)))
            {
                string groupName = $"chartUpdate_{currencyPairId}_{(int)range}";
                await _ordersHubContext.Clients.Group(groupName).SendAsync("chartUpdate", tradeLastCandles[(int)range - 1]);
            }
        }
        //private async Task UpdateDecimalsAsync()
        //{
        //    var abi = await _abiProvider.GetTokenAbiAsync();
        //    var contract = _nethereumClient.Eth.GetContract(abi, _ethSettings.EtherDeltaContractAddress);

        //    if (!_decimals.HasValue) //cached for the instance
        //    {
        //        var decimalsFunc = contract.GetFunction("decimals");
        //        _decimals = await decimalsFunc.CallAsync<int>();
        //    }
        //}
    }
}
