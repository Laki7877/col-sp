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
            this.BrandOldMaps = new HashSet<BrandOldMap>();
            this.CouponBrandMaps = new HashSet<CouponBrandMap>();
            this.ProductHistoryGroups = new HashSet<ProductHistoryGroup>();
            this.ProductStageGroups = new HashSet<ProductStageGroup>();
            this.UserBrandMaps = new HashSet<UserBrandMap>();
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
        public string DescriptionMobileTh { get; set; }
        public string DescriptionMobileEn { get; set; }
        public bool FeatureProductStatus { get; set; }
        public bool BannerStatusEn { get; set; }
        public bool BannerSmallStatusEn { get; set; }
        public bool BannerStatusTh { get; set; }
        public bool BannerSmallStatusTh { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }
        public string MetaTitleEn { get; set; }
        public string MetaTitleTh { get; set; }
        public string MetaDescriptionEn { get; set; }
        public string MetaDescriptionTh { get; set; }
        public string MetaKeyEn { get; set; }
        public string MetaKeyTh { get; set; }
        public string SeoEn { get; set; }
        public string SeoTh { get; set; }
        public bool IsLandingPage { get; set; }
        public Nullable<int> SortById { get; set; }
        public Nullable<int> OldBrandId { get; set; }
        public string Bu { get; set; }
        public string UrlKey { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual SortBy SortBy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandFeatureProduct> BrandFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandImage> BrandImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandOldMap> BrandOldMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponBrandMap> CouponBrandMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryGroup> ProductHistoryGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGroup> ProductStageGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserBrandMap> UserBrandMaps { get; set; }
    }
}
