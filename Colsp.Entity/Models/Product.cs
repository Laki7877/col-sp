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
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.ProductAttributes = new HashSet<ProductAttribute>();
            this.ProductReviews = new HashSet<ProductReview>();
            this.ProductTags = new HashSet<ProductTag>();
            this.ProductVideos = new HashSet<ProductVideo>();
            this.ProductGlobalCatMaps = new HashSet<ProductGlobalCatMap>();
            this.ProductLocalCatMaps = new HashSet<ProductLocalCatMap>();
        }
    
        public string Pid { get; set; }
        public string ParentPid { get; set; }
        public string MasterPid { get; set; }
        public long ProductId { get; set; }
        public int ShopId { get; set; }
        public int ShippingId { get; set; }
        public int GlobalCatId { get; set; }
        public Nullable<int> LocalCatId { get; set; }
        public Nullable<int> AttributeSetId { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string ProdTDNameTh { get; set; }
        public string ProdTDNameEn { get; set; }
        public string JDADept { get; set; }
        public string JDASubDept { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
        public string MobileDescriptionEn { get; set; }
        public string MobileDescriptionTh { get; set; }
        public int ImageCount { get; set; }
        public string FeatureImgUrl { get; set; }
        public int PrepareDay { get; set; }
        public bool LimitIndividualDay { get; set; }
        public int PrepareMon { get; set; }
        public int PrepareTue { get; set; }
        public int PrepareWed { get; set; }
        public int PrepareThu { get; set; }
        public int PrepareFri { get; set; }
        public int PrepareSat { get; set; }
        public int PrepareSun { get; set; }
        public string KillerPoint1En { get; set; }
        public string KillerPoint2En { get; set; }
        public string KillerPoint3En { get; set; }
        public string KillerPoint1Th { get; set; }
        public string KillerPoint2Th { get; set; }
        public string KillerPoint3Th { get; set; }
        public string Installment { get; set; }
        public int TheOneCardEarn { get; set; }
        public string GiftWrap { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public string DimensionUnit { get; set; }
        public decimal Weight { get; set; }
        public string WeightUnit { get; set; }
        public string MetaTitleEn { get; set; }
        public string MetaTitleTh { get; set; }
        public string MetaDescriptionEn { get; set; }
        public string MetaDescriptionTh { get; set; }
        public string MetaKeyEn { get; set; }
        public string MetaKeyTh { get; set; }
        public string SeoEn { get; set; }
        public string SeoTh { get; set; }
        public string IsHasExpiryDate { get; set; }
        public string IsVat { get; set; }
        public string UrlKey { get; set; }
        public int BoostWeight { get; set; }
        public int GlobalBoostWeight { get; set; }
        public bool ExpressDelivery { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal PromotionPrice { get; set; }
        public Nullable<System.DateTime> EffectiveDatePromotion { get; set; }
        public Nullable<System.DateTime> ExpireDatePromotion { get; set; }
        public Nullable<System.DateTime> NewArrivalDate { get; set; }
        public bool DefaultVaraint { get; set; }
        public string Display { get; set; }
        public int MiniQtyAllowed { get; set; }
        public int MaxiQtyAllowed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.DateTime> ExpireDate { get; set; }
        public string SaleUnitTh { get; set; }
        public string SaleUnitEn { get; set; }
        public bool IsNew { get; set; }
        public bool IsClearance { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsOnlineExclusive { get; set; }
        public bool IsOnlyAt { get; set; }
        public string Remark { get; set; }
        public bool IsSell { get; set; }
        public bool IsVariant { get; set; }
        public bool IsMaster { get; set; }
        public int VariantCount { get; set; }
        public bool Visibility { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductReview> ProductReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductTag> ProductTags { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductVideo> ProductVideos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductGlobalCatMap> ProductGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductLocalCatMap> ProductLocalCatMaps { get; set; }
    }
}
