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
    
    public partial class CMSCategoryProductMap
    {
        public int CMSCategoryProductMapId { get; set; }
        public Nullable<int> CMSCategoryId { get; set; }
        public string Pid { get; set; }
        public string ProductBoxBadge { get; set; }
        public Nullable<int> ShopId { get; set; }
        public Nullable<int> Sequence { get; set; }
        public string Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
    }
}
