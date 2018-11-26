using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace dex_webapp.Models
{
    //event Withdraw(address token, address user, uint amount, uint balance);
    public class WithdrawEventModel
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string User { get; set; }
        public string Amount { get; set; }
        public string Balance { get; set; }

        public string TransactionHash { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public long BlockNum { get; set; }
        public long GasPriceWei { get; set; }
        public long GasUsed { get; set; }
    }
}
