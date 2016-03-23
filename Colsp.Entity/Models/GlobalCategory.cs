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
    
    public partial class GlobalCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GlobalCategory()
        {
            this.CouponGlobalCatMaps = new HashSet<CouponGlobalCatMap>();
            this.GlobalCatAttributeSetMaps = new HashSet<GlobalCatAttributeSetMap>();
            this.GlobalCatFeatureProducts = new HashSet<GlobalCatFeatureProduct>();
            this.GlobalCatImages = new HashSet<GlobalCatImage>();
            this.Products = new HashSet<Product>();
            this.ProductStageGroups = new HashSet<ProductStageGroup>();
            this.ShopCommissions = new HashSet<ShopCommission>();
            this.ProductGlobalCatMaps = new HashSet<ProductGlobalCatMap>();
            this.ProductStageGlobalCatMaps = new HashSet<ProductStageGlobalCatMap>();
        }
    
        public int CategoryId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public Nullable<decimal> Commission { get; set; }
        public string UrlKeyEn { get; set; }
        public string UrlKeyTh { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }
        public bool Visibility { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponGlobalCatMap> CouponGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GlobalCatAttributeSetMap> GlobalCatAttributeSetMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GlobalCatFeatureProduct> GlobalCatFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GlobalCatImage> GlobalCatImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGroup> ProductStageGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopCommission> ShopCommissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductGlobalCatMap> ProductGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGlobalCatMap> ProductStageGlobalCatMaps { get; set; }
    }
}
