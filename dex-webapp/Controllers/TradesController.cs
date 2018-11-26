using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Models;
using dex_webapp.Models.ViewModels;
using dex_webapp.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dex_webapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradesController : ControllerBase
    {
        private readonly ITradesService _tradesService;
        private readonly IOHCLService _ohclService;

        public TradesController(ITradesService tradesService, IOHCLService ohclService)
        {
            _tradesService = tradesService;
            _ohclService = ohclService;
        }

        [HttpGet("{token}/{address}")]
        public async Task<IEnumerable<TradeViewModel>> Get(string token, string address)
        {
            return await _tradesService.GetMyTrades(token, address);
        }

        [HttpGet("{token}")]
        public async Task<IEnumerable<TradeViewModel>> Get(string token)
        {
            return await _tradesService.GetTrades(token);
        }

        [HttpGet("markets")]
        public async Task<IEnumerable<MarketViewModel>> GetMarkets()
        {
            return await _tradesService.GetMarkets();
        }

        /// <summary>
        /// Return data to build japanese candlestick charts
        /// </summary>
        /// <param name="range">Range Year = 1, Month = 2, Day = 3, Hour4 = 4, Hour = 5, Minutes30 = 6, Minutes15 = 7, Minutes5 = 8, Minutes3 = 9, Minute = 10</param>
        /// <param name="currencyId">Currency id</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Route("GetOHLC/{range}/{currencyId}/{start}/{end}")]
        public async Task<IEnumerable<ResponseOHLC>> GetOHLCData(MarketDataItemRange range, string currencyId,
            DateTime start,
            DateTime end)
        {
            return await _ohclService.GetOHLCData(range, currencyId, start, end);
        }

        /// <summary>
        /// Return candle at certain time (used in chart)
        /// </summary>
        /// <param name="range">Range Year = 1, Month = 2, Day = 3, Hour4 = 4, Hour = 5, Minutes30 = 6, Minutes15 = 7, Minutes5 = 8, Minutes3 = 9, Minute = 10</param>
        /// <param name="currencyId">Currency id</param>
        /// <param name="before">return only last candle before the specified date</param>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Route("ohlc-last-candle/{range}/{currencyId}/{before?}")]
        public async Task<ResponseOHLC> GetOHLCLastCandle(MarketDataItemRange range, string currencyId,
            DateTime? before = null)
        {
            return await _ohclService.GetOHLCLastCandle(range, currencyId, before);
        }
    }
}
