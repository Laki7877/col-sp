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
    
    public partial class AttributeFilterMap
    {
        public int AttributeValueId { get; set; }
        public int FilterId { get; set; }
        public string BU { get; set; }
    
        public virtual AttributeValue AttributeValue { get; set; }
    }
}
