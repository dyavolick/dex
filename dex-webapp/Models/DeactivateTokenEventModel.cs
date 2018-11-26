using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models
{
    public class DeactivateTokenEventModel
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string Symbol { get; set; }

        public string TransactionHash { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public long BlockNum { get; set; }
        public long GasPriceWei { get; set; }
        public long GasUsed { get; set; }
    }
}
