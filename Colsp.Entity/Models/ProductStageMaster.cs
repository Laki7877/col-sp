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
    
    public partial class ProductStageMaster
    {
        public string MasterPid { get; set; }
        public string ChildPid { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual ProductStage ProductStage { get; set; }
        public virtual ProductStage ProductStage1 { get; set; }
    }
}
