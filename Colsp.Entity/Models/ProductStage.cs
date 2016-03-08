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
    
    public partial class ProductStage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductStage()
        {
            this.BrandFeatureProducts = new HashSet<BrandFeatureProduct>();
            this.GlobalCatFeatureProducts = new HashSet<GlobalCatFeatureProduct>();
            this.LocalCatFeatureProducts = new HashSet<LocalCatFeatureProduct>();
            this.ProductStageAttributes = new HashSet<ProductStageAttribute>();
            this.ProductStageGlobalCatMaps = new HashSet<ProductStageGlobalCatMap>();
            this.ProductStageLocalCatMaps = new HashSet<ProductStageLocalCatMap>();
            this.ProductStageVariants = new HashSet<ProductStageVariant>();
            this.ProductVariants = new HashSet<ProductVariant>();
        }
    
        public int ProductId { get; set; }
        public Nullable<int> GlobalCatId { get; set; }
        public Nullable<int> LocalCatId { get; set; }
        public int ShopId { get; set; }
        public Nullable<int> AttributeSetId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
        public Nullable<int> ShippingId { get; set; }
        public int ImageCount { get; set; }
        public string FeatureImgUrl { get; set; }
        public string Tag { get; set; }
        public decimal PrepareDay { get; set; }
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
        public string UrlEn { get; set; }
        public Nullable<int> BoostWeight { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<bool> ControlFlag1 { get; set; }
        public Nullable<bool> ControlFlag2 { get; set; }
        public Nullable<bool> ControlFlag3 { get; set; }
        public string Remark { get; set; }
        public bool InfoFlag { get; set; }
        public bool ImageFlag { get; set; }
        public bool OnlineFlag { get; set; }
        public bool Visibility { get; set; }
        public string InformationTabStatus { get; set; }
        public string ImageTabStatus { get; set; }
        public string CategoryTabStatus { get; set; }
        public string VariantTabStatus { get; set; }
        public string MoreOptionTabStatus { get; set; }
        public string RejectReason { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual AttributeSet AttributeSet { get; set; }
        public virtual Brand Brand { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandFeatureProduct> BrandFeatureProducts { get; set; }
        public virtual GlobalCategory GlobalCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GlobalCatFeatureProduct> GlobalCatFeatureProducts { get; set; }
        public virtual LocalCategory LocalCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCatFeatureProduct> LocalCatFeatureProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageAttribute> ProductStageAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageGlobalCatMap> ProductStageGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageLocalCatMap> ProductStageLocalCatMaps { get; set; }
        public virtual Shipping Shipping { get; set; }
        public virtual Shop Shop { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVariant> ProductStageVariants { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
