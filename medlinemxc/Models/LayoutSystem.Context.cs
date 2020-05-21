﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class LayoutSystemEntities : DbContext
    {
        public LayoutSystemEntities()
            : base("name=LayoutSystemEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<t_boxingscan> t_boxingscan { get; set; }
        public virtual DbSet<t_lineconf> t_lineconf { get; set; }
        public virtual DbSet<t_lineconfd> t_lineconfd { get; set; }
        public virtual DbSet<t_lineconfh> t_lineconfh { get; set; }
        public virtual DbSet<t_boxingscan_h> t_boxingscan_h { get; set; }
        public virtual DbSet<t_config> t_config { get; set; }
        public virtual DbSet<t_boxingscan_d> t_boxingscan_d { get; set; }
    
        public virtual ObjectResult<sp_dailyline_Result> sp_dailyline(string linea, string turno, Nullable<System.DateTime> fecha)
        {
            var lineaParameter = linea != null ?
                new ObjectParameter("linea", linea) :
                new ObjectParameter("linea", typeof(string));
    
            var turnoParameter = turno != null ?
                new ObjectParameter("turno", turno) :
                new ObjectParameter("turno", typeof(string));
    
            var fechaParameter = fecha.HasValue ?
                new ObjectParameter("fecha", fecha) :
                new ObjectParameter("fecha", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_dailyline_Result>("sp_dailyline", lineaParameter, turnoParameter, fechaParameter);
        }
    
        public virtual ObjectResult<cleanroomview1_Result> cleanroomview1(Nullable<System.DateTime> fecha)
        {
            var fechaParameter = fecha.HasValue ?
                new ObjectParameter("fecha", fecha) :
                new ObjectParameter("fecha", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<cleanroomview1_Result>("cleanroomview1", fechaParameter);
        }
    }
}
