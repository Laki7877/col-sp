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
    
    public partial class CartDiscount
    {
        public int CartId { get; set; }
        public string PId { get; set; }
        public string PromotionNo { get; set; }
        public string PromotionType { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal DiscountAmt { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public string UpdateBy { get; set; }
    }
}
