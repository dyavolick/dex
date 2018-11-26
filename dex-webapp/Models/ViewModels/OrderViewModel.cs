using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string TokenGet { get; set; }

        public string AmountGet { get; set; }
        public string TokenGive { get; set; }

        public string AmountGive { get; set; }
        public string Expires { get; set; }
        public string Nonce { get; set; }

        public string Available { get; set; }
        public string Filled { get; set; }
        public string User { get; set; }

        public string Hash { get; set; }
    }
}
