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
    
    public partial class CMSMasterCategoryMap
    {
        public int CMSMasterCategoryMapId { get; set; }
        public int CMSMasterId { get; set; }
        public int CMSCategoryId { get; set; }
        public int ShopId { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
    }
}
