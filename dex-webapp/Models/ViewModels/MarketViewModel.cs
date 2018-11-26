using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models.ViewModels
{
    public class MarketViewModel
    {
        public string Symbol { get; set; }

        public decimal Price { get; set; }

        /// 24hour volume
        public string Volume { get; set; }

        /// 24hour price change
        public decimal Change { get; set; }
    }
}
