using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models.ViewModels
{
    public class TradeViewModel
    {
        public string TokenGet { get; set; }

        public string AmountGet { get; set; }
        public string TokenGive { get; set; }

        public string AmountGive { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
