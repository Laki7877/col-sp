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
    
    public partial class ProductHistoryAttribute
    {
        public long HistoryId { get; set; }
        public string Pid { get; set; }
        public int AttributeId { get; set; }
        public string ValueEn { get; set; }
        public string ValueTh { get; set; }
        public Nullable<int> AttributeValueId { get; set; }
        public bool CheckboxValue { get; set; }
        public int Position { get; set; }
        public bool IsAttributeValue { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual Attribute Attribute { get; set; }
        public virtual ProductHistory ProductHistory { get; set; }
    }
}
