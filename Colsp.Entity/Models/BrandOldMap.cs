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
    
    public partial class BrandOldMap
    {
        public int BrandId { get; set; }
        public int OldBrandId { get; set; }
        public string DbName { get; set; }
    
        public virtual Brand Brand { get; set; }
    }
}
