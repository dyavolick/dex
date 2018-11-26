using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace dex_webapp.Models
{
    public class TokenModel
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string Symbol { get; set; }
        public string Image { get; set; }
        public int Status { get; set; }
        public DateTime StatusDateUpdate { get; set; }
        public long StatusBlockUpdate { get; set; }
    }
}
