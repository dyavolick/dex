using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models.Ethereum
{
    //event ActivateToken(address token, string name);
    public class ActivateTokenEvent
    {
        [Parameter("address", "token")]
        public string Token { get; set; }

        [Parameter("string", "symbol")]
        public string Symbol { get; set; }

    }
}
