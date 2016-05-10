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
    
    public partial class Shop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Shop()
        {
            this.Coupons = new HashSet<Coupon>();
            this.CouponShopMaps = new HashSet<CouponShopMap>();
            this.LocalCategories = new HashSet<LocalCategory>();
            this.LocalCatFeatureProducts = new HashSet<LocalCatFeatureProduct>();
            this.LocalCatImages = new HashSet<LocalCatImage>();
            this.NewsletterShopMaps = new HashSet<NewsletterShopMap>();
            this.ProductReviews = new HashSet<ProductReview>();
            this.ProductStages = new HashSet<ProductStage>();
            this.ProductStageGroups = new HashSet<ProductStageGroup>();
            this.ProductStageImages = new HashSet<ProductStageImage>();
            this.ProductStageRelateds = new HashSet<ProductStageRelated>();
            this.ProductStageVideos = new HashSet<ProductStageVideo>();
            this.ProductTmps = new HashSet<ProductTmp>();
            this.ShopCommissions = new HashSet<ShopCommission>();
            this.ShopComponentMaps = new HashSet<ShopComponentMap>();
            this.ShopImages = new HashSet<ShopImage>();
            this.ShopUserGroupMaps = new HashSet<ShopUserGroupMap>();
            this.UserShopMaps = new HashSet<UserShopMap>();
        }
    
        public int ShopId { get; set; }
        public string VendorId { get; set; }
        public Nullable<int> ShopOwner { get; set; }
        public string ShopNameEn { get; set; }
        public string ShopNameTh { get; set; }
        public int MaxLocalCategory { get; set; }
        public string GiftWrap { get; set; }
        public string TaxInvoice { get; set; }
        public string ShopGroup { get; set; }
        public string ShopImageUrl { get; set; }
        public string ShopDescriptionEn { get; set; }
        public string ShopDescriptionTh { get; set; }
        public string DomainName { get; set; }
        public string ShopAddress { get; set; }
        public string Facebook { get; set; }
        public string YouTube { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Pinterest { get; set; }
        public int StockAlert { get; set; }
        public decimal Commission { get; set; }
        public Nullable<int> ShopTypeId { get; set; }
        public string UrlKey { get; set; }
        public string FloatMessageEn { get; set; }
        public string FloatMessageTh { get; set; }
        public Nullable<int> ThemeId { get; set; }
        public string TaxPayerId { get; set; }
        public string TermPaymentCode { get; set; }
        public string Payment { get; set; }
        public string VendorTaxRateCode { get; set; }
        public string WithholdingTaxCode { get; set; }
        public string VendorAddressLine1 { get; set; }
        public string VendorAddressLine2 { get; set; }
        public string VendorAddressLine3 { get; set; }
        public Nullable<int> CityId { get; set; }
        public Nullable<int> ProvinceId { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Telex { get; set; }
        public string OverseasVendorIndicator { get; set; }
        public string BankNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string RemittanceFaxNumber { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        public virtual BankDetail BankDetail { get; set; }
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Coupon> Coupons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CouponShopMap> CouponShopMaps { get; set; }
        public virtual District District { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCategory> LocalCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCatFeatureProduct> LocalCatFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCatImage> LocalCatImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NewsletterShopMap> NewsletterShopMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductReview> ProductReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStage> ProductStages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGroup> ProductStageGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageImage> ProductStageImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageRelated> ProductStageRelateds { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVideo> ProductStageVideos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductTmp> ProductTmps { get; set; }
        public virtual Province Province { get; set; }
        public virtual ShopType ShopType { get; set; }
        public virtual TermPayment TermPayment { get; set; }
        public virtual Theme Theme { get; set; }
        public virtual User User { get; set; }
        public virtual VendorTaxRate VendorTaxRate { get; set; }
        public virtual WithholdingTax WithholdingTax { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopCommission> ShopCommissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopComponentMap> ShopComponentMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopImage> ShopImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserShopMap> UserShopMaps { get; set; }
    }
}
