//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Entity.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CMSHistoryLog
    {
        public int CMSHistoryLogId { get; set; }
        public Nullable<int> ChangeId { get; set; }
        public string CMSTableLog { get; set; }
        public string TransactionLog { get; set; }
        public string DetailLog { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public string CreateIP { get; set; }
    }
}