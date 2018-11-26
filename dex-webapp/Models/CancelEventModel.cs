using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace dex_webapp.Models
{
    //event Cancel(address tokenGet, uint amountGet, address tokenGive, uint amountGive, uint expires, uint nonce, address user);
    public class CancelEventModel
    {
        [Key]
        public int Id { get; set; }
        public string TokenGet { get; set; }
        public string AmountGet { get; set; }
        public string TokenGive { get; set; }
        public string AmountGive { get; set; }
        public string Expires { get; set; }
        public string Nonce { get; set; }
        public string User { get; set; }

        public string Hash { get; set; }
        public string TransactionHash { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public long BlockNum { get; set; }
        public long GasPriceWei { get; set; }
        public long GasUsed { get; set; }
    }
}
