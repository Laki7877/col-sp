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
    
    public partial class ProductReview
    {
        public long ProductReviewId { get; set; }
        public string CustomerId { get; set; }
        public decimal DeliverySpeed { get; set; }
        public decimal ProductContent { get; set; }
        public decimal ProductValidity { get; set; }
        public decimal Packaging { get; set; }
        public string Pid { get; set; }
        public int ShopId { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual Shop Shop { get; set; }
        public virtual Product Product { get; set; }
    }
}
