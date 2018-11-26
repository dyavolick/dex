using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace dex_webapp.Models.Ethereum
{
    //event Deposit(address token, address user, uint amount, uint balance);
    public class DepositEvent
    {
        [Parameter("address", "token")]
        public string Token { get; set; }

        [Parameter("address", "user")]
        public string User { get; set; }

        [Parameter("uint", "amount")]
        public BigInteger Amount { get; set; }

        [Parameter("uint", "balance")]
        public BigInteger Balance { get; set; }
    }
}
