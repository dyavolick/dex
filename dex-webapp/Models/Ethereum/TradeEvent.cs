using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace dex_webapp.Models.Ethereum
{
    //event Trade(address tokenGet, uint amountGet, address tokenGive, uint amountGive, address get, address give);
    public class TradeEvent
    {
        [Parameter("address", "tokenGet")]
        public string TokenGet { get; set; }

        [Parameter("uint", "amountGet")]
        public BigInteger AmountGet { get; set; }

        [Parameter("address", "tokenGive")]
        public string TokenGive { get; set; }

        [Parameter("uint", "amountGive")]
        public BigInteger AmountGive { get; set; }

        [Parameter("address", "get")]
        public string Get { get; set; }

        [Parameter("address", "give")]
        public string Give { get; set; }
    }
}
