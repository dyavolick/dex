using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Services;

namespace dex_webapp.Models
{
    public class OhlcData
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Currency id
        /// </summary>
        public string CurrencyId { get; set; }

        /// <summary>
        /// Date time
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Date time range
        /// </summary>
        public MarketDataItemRange Range { get; set; }

        /// <summary>
        /// Min value
        /// </summary>
        public decimal Min { get; set; }

        /// <summary>
        /// Max value
        /// </summary>
        public decimal Max { get; set; }

        /// <summary>
        /// Open
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Close
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Volume in base currency
        /// </summary>
        public decimal VolumeBase { get; set; }
    }
}
