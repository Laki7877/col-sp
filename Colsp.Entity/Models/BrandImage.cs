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
    
    public partial class BrandImage
    {
        public int BrandImageId { get; set; }
        public int BrandId { get; set; }
        public int Position { get; set; }
        public string ImageUrl { get; set; }
        public string EnTh { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual Brand Brand { get; set; }
    }
}
