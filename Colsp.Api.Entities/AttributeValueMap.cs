//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Api.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class AttributeValueMap
    {
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual Attribute Attribute { get; set; }
        public virtual AttributeValue AttributeValue { get; set; }
    }
}
