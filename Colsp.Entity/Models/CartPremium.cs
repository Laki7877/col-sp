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
    
    public partial class CartPremium
    {
        public int CartId { get; set; }
        public string PId { get; set; }
        public string GetPId { get; set; }
        public int GetQty { get; set; }
        public string PromotionNo { get; set; }
        public string PromotionDescription { get; set; }
        public System.DateTime CreateOn { get; set; }
        public System.DateTime CreateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public Nullable<System.DateTime> UpdateBy { get; set; }
    }
}
