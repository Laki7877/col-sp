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
    
    public partial class Attribute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Attribute()
        {
            this.AttributeSetMaps = new HashSet<AttributeSetMap>();
            this.AttributeValueMaps = new HashSet<AttributeValueMap>();
            this.ProductStageAttributes = new HashSet<ProductStageAttribute>();
        }
    
        public int AttributeId { get; set; }
        public string AttributeNameEn { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public bool Required { get; set; }
        public bool Filterable { get; set; }
        public bool VariantStatus { get; set; }
        public string DataType { get; set; }
        public string VariantDataType { get; set; }
        public string DataValidation { get; set; }
        public string DefaultValue { get; set; }
        public bool ShowAdminFlag { get; set; }
        public bool ShowGlobalSearchFlag { get; set; }
        public bool ShowLocalSearchFlag { get; set; }
        public bool ShowGlobalFilterFlag { get; set; }
        public bool ShowLocalFilterFlag { get; set; }
        public bool AllowHtmlFlag { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AttributeSetMap> AttributeSetMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AttributeValueMap> AttributeValueMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageAttribute> ProductStageAttributes { get; set; }
    }
}
