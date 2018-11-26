using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Data;
using dex_webapp.Models;
using Microsoft.EntityFrameworkCore;

namespace dex_webapp.Services
{
    public interface IOHCLService
    {
        Task<IEnumerable<ResponseOHLC>> GetOHLCData(MarketDataItemRange range, string currencyId, DateTime start,
            DateTime end);

        Task<ResponseOHLC> GetOHLCLastCandle(MarketDataItemRange range, string currencyId,
            DateTime? before = null);
        void WriteOHLC(decimal amount, decimal volume, string currency, DateTime date);

        Task<List<ResponseOHLC>> GetChartLastCandles(string currencyPairId);
    }
    public class OHCLService : IOHCLService
    {
        private readonly ApplicationDbContext _context;

        public OHCLService(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="currencyId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ResponseOHLC>> GetOHLCData(MarketDataItemRange range, string currencyId, DateTime start, DateTime end)
        {
            var result = await _context.OHLCData
                .Where(a => a.Range == range && a.CurrencyId == currencyId && a.Date >= start && a.Date <= end)
                .Select(a => new ResponseOHLC
                {
                    Date = a.Date,
                    Close = a.Close,
                    Open = a.Open,
                    Max = a.Max,
                    Min = a.Min,
                    Volume = a.Volume,
                    VolumeBase = a.VolumeBase
                }).OrderBy(a => a.Date).ToListAsync();
            return result;
        }

        public async Task<ResponseOHLC> GetOHLCLastCandle(MarketDataItemRange range, string currencyId, DateTime? before = null)
        {
            var result = await _context.OHLCData
                .Where(a => a.Range == range && a.CurrencyId == currencyId && (!before.HasValue || a.Date < before))
                .Select(a => new ResponseOHLC
                {
                    Date = a.Date,
                    Close = a.Close,
                    Open = a.Open,
                    Max = a.Max,
                    Min = a.Min,
                    Volume = a.Volume,
                    VolumeBase = a.VolumeBase
                }).OrderByDescending(a => a.Date).FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="volume"></param>
        /// <param name="currency"></param>
        /// <param name="date"></param>
        public void WriteOHLC(decimal amount, decimal volume, string currency, DateTime date)
        {
            PutData(MarketDataItemRange.Year, amount, volume, currency, date);
            PutData(MarketDataItemRange.Month, amount, volume, currency, date);
            PutData(MarketDataItemRange.Day, amount, volume, currency, date);
            PutData(MarketDataItemRange.Hour4, amount, volume, currency, date);
            PutData(MarketDataItemRange.Hour, amount, volume, currency, date);
            PutData(MarketDataItemRange.Minutes30, amount, volume, currency, date);
            PutData(MarketDataItemRange.Minutes15, amount, volume, currency, date);
            PutData(MarketDataItemRange.Minutes5, amount, volume, currency, date);
            PutData(MarketDataItemRange.Minutes3, amount, volume, currency, date);
            PutData(MarketDataItemRange.Minute, amount, volume, currency, date);
        }

        public async Task<List<ResponseOHLC>> GetChartLastCandles(string currencyPairId)
        {
            var result = new List<ResponseOHLC>();
            foreach (MarketDataItemRange range in Enum.GetValues(typeof(MarketDataItemRange)))
            {
                ResponseOHLC candle = await _context.OHLCData
                    .Where(a => a.Range == range && a.CurrencyId == currencyPairId)
                    .Select(a => new ResponseOHLC
                    {
                        Date = a.Date,
                        Close = a.Close,
                        Open = a.Open,
                        Max = a.Max,
                        Min = a.Min,
                        Volume = a.Volume,
                        VolumeBase = a.VolumeBase
                    }).OrderByDescending(a => a.Date).FirstOrDefaultAsync();
                result.Add(candle ?? new ResponseOHLC());
            }
            return result;
        }

        /// <summary>
        /// Update OHLC data
        /// </summary>
        /// <param name="range">Range</param>
        /// <param name="amount">Amount</param>
        /// <param name="volume">Volume</param>
        /// <param name="currency">Currency</param>
        /// <param name="date">Date</param>
        public void PutData(MarketDataItemRange range, decimal amount, decimal volume, string currency, DateTime date)
        {
            OhlcData di = GetDataItem(range, currency, date);

            if (di == null)
            {
                di = CreateDataItem(range, amount, volume, currency, date);
            }
            else
            {
                UpdateDataItem(di, amount, volume);
            }

            _context.SaveChanges();
        }
        private OhlcData CreateDataItem(MarketDataItemRange range, decimal amount, decimal volume, string currency, DateTime date)
        {
            OhlcData result = new OhlcData();

            result.CurrencyId = currency;
            result.Volume = volume;
            result.VolumeBase = volume * amount;

            result.Max = amount;
            result.Min = amount;
            result.Close = amount;
            result.Date = date;
            result.Range = range;

            _context.Add(result);
            _context.SaveChanges();

            OhlcData previous = GetPreviousDataItem(result);
            if (previous == null)
            {
                result.Open = 0;
            }
            else
            {
                result.Open = previous.Close;
            }

            return result;
        }

        /// <summary>
        /// Get previous OHLC data item
        /// </summary>
        /// <param name="item">OHLC data item detalis</param>
        /// <returns></returns>
        private OhlcData GetPreviousDataItem(OhlcData item)
        {
            OhlcData result = null;

            result = _context.OHLCData.Where(a => a.Range == item.Range && a.CurrencyId == item.CurrencyId && a.Id < item.Id).OrderByDescending(a => a.Id).Take(1).FirstOrDefault();

            return result;
        }

        private OhlcData UpdateDataItem(OhlcData item, decimal amount, decimal volume)
        {
            item.Volume += volume;
            item.VolumeBase += volume * amount;

            if (item.Max < amount)
            {
                item.Max = amount;
            }

            if (item.Min > amount)
            {
                item.Min = amount;
            }

            item.Close = amount;

            return item;
        }
        /// <summary>
        /// Get OHLC element from database
        /// </summary>
        /// <param name="range">Range</param>
        /// <param name="currency">Currency</param>
        /// <param name="date">Date</param>
        /// <returns></returns>
        private OhlcData GetDataItem(MarketDataItemRange range, string currency, DateTime date)
        {
            OhlcData result = null;

            if (range == MarketDataItemRange.Year)
            {
                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2}", range, currency, date.Year).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Month)
            {
                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3}", range, currency, date.Year, date.Month).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Day)
            {
                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4}", range, currency, date.Year, date.Month, date.Day).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Hour4)
            {
                int hour = date.Hour;
                int range_start, range_end;

                BuildRange(1, 23, 6, hour, out range_start, out range_end);

                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and ((date_part('hour', \"Date\") >= {5}) and (date_part('hour', \"Date\") < {6}))", range, currency, date.Year, date.Month, date.Day, range_start, range_end).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Hour)
            {
                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5}", range, currency, date.Year, date.Month, date.Day, date.Hour).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Minute)
            {
                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5} and date_part('minute', \"Date\")={6}", range, currency, date.Year, date.Month, date.Day, date.Hour, date.Minute).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Minutes30)
            {
                int minute = date.Minute;
                int range_start, range_end;

                BuildRange(1, 59, 2, minute, out range_start, out range_end);

                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5} and (date_part('minute', \"Date\")>={6} and date_part('minute', \"Date\")<{7})", range, currency, date.Year, date.Month, date.Day, date.Hour, range_start, range_end).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Minutes15)
            {
                int minute = date.Minute;
                int range_start, range_end;

                BuildRange(1, 59, 4, minute, out range_start, out range_end);

                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5} and (date_part('minute', \"Date\")>={6} and date_part('minute', \"Date\")<{7})", range, currency, date.Year, date.Month, date.Day, date.Hour, range_start, range_end).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Minutes5)
            {
                int minute = date.Minute;
                int range_start, range_end;

                BuildRange(1, 59, 12, minute, out range_start, out range_end);

                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5} and (date_part('minute', \"Date\")>={6} and date_part('minute', \"Date\")<{7})", range, currency, date.Year, date.Month, date.Day, date.Hour, range_start, range_end).FirstOrDefault();
            }

            if (range == MarketDataItemRange.Minutes3)
            {
                int minute = date.Minute;
                int range_start, range_end;

                BuildRange(1, 59, 20, minute, out range_start, out range_end);

                result = _context.OHLCData.FromSql("select * from \"OHLCData\" where \"Range\"={0} and \"CurrencyId\"={1} and date_part('year', \"Date\")={2} and date_part('month', \"Date\")={3} and date_part('day', \"Date\")={4} and date_part('hour', \"Date\")={5} and (date_part('minute', \"Date\")>={6} and date_part('minute', \"Date\")<{7})", range, currency, date.Year, date.Month, date.Day, date.Hour, range_start, range_end).FirstOrDefault();
            }

            return result;
        }
        /// <summary>
        /// Calculates if value is in diapason
        /// </summary>
        /// <param name="start">Minimum diapason value (0) for minute</param>
        /// <param name="end">MAximum diapason value (59) for minute</param>
        /// <param name="length">Diapason length 30 for minute</param>
        /// <param name="value">Value to check</param>
        /// <param name="range_start">Diapason start</param>
        /// <param name="range_end">Diapason end</param>
        public static void BuildRange(int start, int end, int length, int value, out int range_start, out int range_end)
        {
            int range_index = 0;

            int range_length = (start + end) / length;

            for (int i = 0; i < length + 1; i++)
            {
                if ((value >= (i * range_length) + 1) && (value <= (i + 1) * range_length))
                {
                    range_index = i;
                    break;
                }
            }

            range_start = range_index * range_length;
            range_end = (range_index + 1) * range_length;
        }
    }
    public enum MarketDataItemRange
    {
        Year = 1, Month = 2, Day = 3, Hour4 = 4, Hour = 5, Minutes30 = 6, Minutes15 = 7, Minutes5 = 8, Minutes3 = 9, Minute = 10
    }
}
