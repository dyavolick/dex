using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace dex_webapp.Models.Ethereum
{
    //event Cancel(address tokenGet, uint amountGet, address tokenGive, uint amountGive, uint expires, uint nonce, address user);
    public class CancelEvent
    {
        [Parameter("address", "tokenGet")]
        public string TokenGet { get; set; }

        [Parameter("uint", "amountGet")]
        public BigInteger AmountGet { get; set; }

        [Parameter("address", "tokenGive")]
        public string TokenGive { get; set; }

        [Parameter("uint", "amountGive")]
        public BigInteger AmountGive { get; set; }

        [Parameter("uint", "expires")]
        public BigInteger Expires { get; set; }

        [Parameter("uint", "nonce")]
        public BigInteger Nonce { get; set; }

        [Parameter("address", "user")]
        public string User { get; set; }

    }
}
