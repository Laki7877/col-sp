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
    
    public partial class Migrate_TBDepartment
    {
        public int DepartmentId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameEN { get; set; }
        public string MetaKeyword { get; set; }
        public string MetaDescription { get; set; }
        public string UrlName { get; set; }
        public string UrlNameEN { get; set; }
        public string Title { get; set; }
        public string VirtualPath { get; set; }
        public byte Status { get; set; }
        public int SeqNo { get; set; }
        public Nullable<int> Catlv1Id { get; set; }
        public Nullable<int> Catlv2Id { get; set; }
        public Nullable<int> Catlv3Id { get; set; }
        public Nullable<int> DepartmentId_ref { get; set; }
        public Nullable<int> RootDeptId { get; set; }
    }
}
