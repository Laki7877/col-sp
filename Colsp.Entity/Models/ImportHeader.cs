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
    
    public partial class ImportHeader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImportHeader()
        {
            this.ShopGroups = new HashSet<ShopGroup>();
        }
    
        public int ImportHeaderId { get; set; }
        public string HeaderName { get; set; }
        public string Description { get; set; }
        public string AcceptedValue { get; set; }
        public string Example { get; set; }
        public string Note { get; set; }
        public string GroupName { get; set; }
        public int Position { get; set; }
        public string MapName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopGroup> ShopGroups { get; set; }
    }
}
