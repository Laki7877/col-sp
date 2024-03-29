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
    
    public partial class Deal
    {
        public int DealId { get; set; }
        public string PID { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string Description { get; set; }
        public string DescriptionEN { get; set; }
        public int AvailableQty { get; set; }
        public int ReserveQty { get; set; }
        public System.DateTime DealEffectiveDate { get; set; }
        public System.DateTime DealExpiryDate { get; set; }
        public bool Status { get; set; }
        public decimal DealPrice { get; set; }
        public int LimitQty { get; set; }
        public int SeqNo { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public bool IsLockUser { get; set; }
    }
}
