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
    
    public partial class ProductHistoryImage
    {
        public long HistoryId { get; set; }
        public long ImageId { get; set; }
        public string Pid { get; set; }
        public string ImageName { get; set; }
        public int SeqNo { get; set; }
        public bool Thumbnail { get; set; }
        public bool Normal { get; set; }
        public bool Large { get; set; }
        public bool Zoom { get; set; }
        public int ShopId { get; set; }
        public bool FeatureFlag { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public long ImageId { get; set; }
        public int SeqNo { get; set; }
        public bool Thumbnail { get; set; }
        public bool Normal { get; set; }
        public bool Large { get; set; }
        public bool Zoom { get; set; }
    
        public virtual ProductHistory ProductHistory { get; set; }
    }
}
