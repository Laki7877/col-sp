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
    
    public partial class ThemeComponent
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ThemeComponent()
        {
            this.ThemeComponentMaps = new HashSet<ThemeComponentMap>();
        }
    
        public int ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ThemeComponentMap> ThemeComponentMaps { get; set; }
    }
}
