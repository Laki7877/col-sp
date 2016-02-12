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
            this.ProductAttributes = new HashSet<ProductAttribute>();
            this.ProductHistoryAttributes = new HashSet<ProductHistoryAttribute>();
            this.ProductHistoryVariants = new HashSet<ProductHistoryVariant>();
            this.ProductStageAttributes = new HashSet<ProductStageAttribute>();
            this.ProductStageVariants = new HashSet<ProductStageVariant>();
            this.ProductStageVariants1 = new HashSet<ProductStageVariant>();
            this.ProductVariants = new HashSet<ProductVariant>();
            this.ProductVariants1 = new HashSet<ProductVariant>();
        }
    //
        public int AttributeId { get; set; }
        public string AttributeNameEn { get; set; }
        public string AttributeNameTh { get; set; }
        public string AttributeUnitEn { get; set; }
        public string AttributeUnitTh { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public Nullable<bool> VariantStatus { get; set; }
        public string DataType { get; set; }
        public string VariantDataType { get; set; }
        public string DataValidation { get; set; }
        public string DefaultValue { get; set; }
        public Nullable<bool> ShowAdminFlag { get; set; }
        public Nullable<bool> ShowGlobalSearchFlag { get; set; }
        public Nullable<bool> ShowLocalSearchFlag { get; set; }
        public Nullable<bool> ShowGlobalFilterFlag { get; set; }
        public Nullable<bool> ShowLocalFilterFlag { get; set; }
        public string Status { get; set; }
        public Nullable<bool> AllowHtmlFlag { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AttributeSetMap> AttributeSetMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AttributeValueMap> AttributeValueMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryAttribute> ProductHistoryAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryVariant> ProductHistoryVariants { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageAttribute> ProductStageAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVariant> ProductStageVariants { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVariant> ProductStageVariants1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductVariant> ProductVariants1 { get; set; }
    }
}
