using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models
{
    public class OrderFilledModel
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string AmountAvailable { get; set; }
        public string AmountFilled { get; set; }
        public bool IsDone { get; set; }
    }
}
