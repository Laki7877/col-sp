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
    
    public partial class LocalCatImage
    {
        public int CategoryImageId { get; set; }
        public int CategoryId { get; set; }
        public int ShopId { get; set; }
        public int Position { get; set; }
        public string ImageUrl { get; set; }
        public string EnTh { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual LocalCategory LocalCategory { get; set; }
        public virtual Shop Shop { get; set; }
    }
}
