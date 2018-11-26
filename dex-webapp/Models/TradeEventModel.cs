using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models
{
    public class TradeEventModel
    {
        [Key]
        public int Id { get; set; }
        public string TokenGet { get; set; }      
        public string AmountGet { get; set; }    
        public string TokenGive { get; set; }    
        public string AmountGive { get; set; }    
        public string Get { get; set; }
        public string Give { get; set; }

        public string TransactionHash { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public long BlockNum { get; set; }
        public long GasPriceWei { get; set; }
        public long GasUsed { get; set; }
    }
}
