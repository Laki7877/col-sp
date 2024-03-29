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
    
    public partial class LocalCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LocalCategory()
        {
            this.CouponLocalCatMaps = new HashSet<CouponLocalCatMap>();
            this.CouponLocalCatPidMaps = new HashSet<CouponLocalCatPidMap>();
            this.LocalCatFeatureProducts = new HashSet<LocalCatFeatureProduct>();
            this.LocalCatImages = new HashSet<LocalCatImage>();
            this.ProductHistoryGroups = new HashSet<ProductHistoryGroup>();
            this.ProductStageGroups = new HashSet<ProductStageGroup>();
            this.ProductHistoryLocalCatMaps = new HashSet<ProductHistoryLocalCatMap>();
            this.ProductLocalCatMaps = new HashSet<ProductLocalCatMap>();
            this.ProductStageLocalCatMaps = new HashSet<ProductStageLocalCatMap>();
        }
    
        public int CategoryId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string UrlKey { get; set; }
        public int ShopId { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
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
        public bool Visibility { get; set; }
        public bool IsLandingPage { get; set; }
        public Nullable<int> SortById { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponLocalCatMap> CouponLocalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponLocalCatPidMap> CouponLocalCatPidMaps { get; set; }
        public virtual Shop Shop { get; set; }
        public virtual SortBy SortBy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCatFeatureProduct> LocalCatFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCatImage> LocalCatImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryGroup> ProductHistoryGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGroup> ProductStageGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryLocalCatMap> ProductHistoryLocalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductLocalCatMap> ProductLocalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageLocalCatMap> ProductStageLocalCatMaps { get; set; }
    }
}
