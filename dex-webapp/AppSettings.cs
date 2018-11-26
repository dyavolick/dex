using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp
{

        public class AbiLocation
        {
            public string Folder { get; set; }
            public string EtherDeltaABIFileName { get; set; }
        }

        public class EthereumSettings
        {
            public string EthereumRpcNodeUrl { get; set; }
            public long EthereumSearchStartBlockNumber { get; set; }
            public string EtherDeltaContractAddress { get; set; }
        }

        public class EtherscanSettings
        {
            public string ApiKey { get; set; }
            public string ApiUrl { get; set; }
            public string Domain { get; set; }
        }

        public class BackgroundScannerSettings
        {
            public int ScanIntervalMilliseconds { get; set; }
        }
    
}
