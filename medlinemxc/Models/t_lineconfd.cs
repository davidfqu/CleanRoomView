//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace medlinemxc.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_lineconfd
    {
        public string clave { get; set; }
        public decimal consec { get; set; }
        public string line { get; set; }
    
        public virtual t_lineconf t_lineconf { get; set; }
    }
}
