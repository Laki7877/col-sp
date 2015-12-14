//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Shop()
        {
            this.Local_Category = new HashSet<Local_Category>();
            this.Products = new HashSet<Product>();
        }
    
        public int shop_id { get; set; }
        public Nullable<int> shop_owner { get; set; }
        public string shop_name_en { get; set; }
        public string shop_name_th { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_dt { get; set; }
        public string updated_by { get; set; }
        public Nullable<System.DateTime> updated_dt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Local_Category> Local_Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
        public virtual User User { get; set; }
    }
}
