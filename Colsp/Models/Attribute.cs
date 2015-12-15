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
    
    public partial class Attribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Attribute()
        {
            this.Product_Attribute = new HashSet<Product_Attribute>();
            this.Product_Variant = new HashSet<Product_Variant>();
            this.Attribute_Set = new HashSet<Attribute_Set>();
            this.Attribute_Value = new HashSet<Attribute_Value>();
        }
    
        public int attribute_id { get; set; }
        public string attribute_name_en { get; set; }
        public string attribute_name_th { get; set; }
        public Nullable<bool> variant_status { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_dt { get; set; }
        public string updated_by { get; set; }
        public Nullable<System.DateTime> updated_dt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product_Attribute> Product_Attribute { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product_Variant> Product_Variant { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Attribute_Set> Attribute_Set { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Attribute_Value> Attribute_Value { get; set; }
    }
}
