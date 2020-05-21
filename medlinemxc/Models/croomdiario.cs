using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace medlinemxc.Models
{
    public class croomdiario
    {
        public string linea { get; set; }
        public string tipo { get; set; }
        public decimal cajas { get; set; }
        public decimal meta { get; set; }
        public decimal meta_actual { get; set; }
        public int kits { get; set; }
        public int wos { get; set; }
        public int tmuerto { get; set; }

    }
}