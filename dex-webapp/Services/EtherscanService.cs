using dex_webapp.Extensions;
using dex_webapp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dex_webapp.Services
{
    public class EtherscanService
    {
        private readonly ILogger<EtherscanService> _logger;
        private readonly HttpClient _client;
        private readonly EtherscanSettings _options;

        public EtherscanService(IOptions<EtherscanSettings> options, ILogger<EtherscanService> logger)
        {
            _logger = logger;
            _options = options.Value;
            _client = new HttpClient();
        }

        public async Task<JArray> GetEventsAsync(NewFilterInput filterInput, long blockFrom, long blockTo)
        {
            var parameters = new
            {
                module = "logs",
                action = "getLogs",
                fromBlock = blockFrom,
                toBlock = blockTo,
                address = filterInput.Address.First(),
                apikey = _options.ApiKey,
                topic0 = filterInput.Topics.Length > 0 ? filterInput.Topics[0] : null,
                topic1 = filterInput.Topics.Length > 1 ? filterInput.Topics[1] : null,
                topic2 = filterInput.Topics.Length > 2 ? filterInput.Topics[2] : null
            };

            var url = _options.ApiUrl + QueryStringHelper.ToQueryString(parameters);

            var response = await GetJsonTimedAsync<EventsResult>(url);

            return response.Result;
        }

        protected DateTime LastApiCall { get; set; } = DateTime.MinValue;
        protected TimeSpan ApiInterval { get; set; } = TimeSpan.FromMilliseconds(250);
        protected object LockApi = new object();

        protected async Task<T> GetJsonTimedAsync<T>(string url)
        {
            return await Task.Run(() => GetJsonTimed<T>(url));
        }

        protected T GetJsonTimed<T>(string url)
        {
            lock (LockApi)
            {
                var fromLastCall = DateTime.Now - LastApiCall;
                if (fromLastCall < ApiInterval)
                {
                    _logger.LogInformation($"Delaying etherscan call for {(ApiInterval - fromLastCall).TotalMilliseconds:F1} milliseconds");
                    Thread.Sleep(ApiInterval - fromLastCall);
                }

                _logger.LogInformation("Calling etherscan: " + url);
                var result = _client.GetJsonAsync<T>(url).Result;
                LastApiCall = DateTime.Now;

                return result;
            }
        }
    }

    public class EventsResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public JArray Result { get; set; }
    }
}
