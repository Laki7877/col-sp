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
    
    public partial class Brand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Brand()
        {
            this.BrandFeatureProducts = new HashSet<BrandFeatureProduct>();
            this.BrandImages = new HashSet<BrandImage>();
            this.CouponBrandMaps = new HashSet<CouponBrandMap>();
            this.Products = new HashSet<Product>();
            this.ProductHistories = new HashSet<ProductHistory>();
            this.ProductStages = new HashSet<ProductStage>();
        }
    
        public int BrandId { get; set; }
        public string BrandNameEn { get; set; }
        public string BrandNameTh { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public string PicUrl { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }
        public string MetaTitleEn { get; set; }
        public string MetaTitleTh { get; set; }
        public string MetaDescriptionEn { get; set; }
        public string MetaDescriptionTh { get; set; }
        public string MetaKeyEn { get; set; }
        public string MetaKeyTh { get; set; }
        public string UrlEn { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandFeatureProduct> BrandFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandImage> BrandImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponBrandMap> CouponBrandMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistory> ProductHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
