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
    
    public partial class ProductHistory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductHistory()
        {
            this.ProductHistoryGlobalCatMaps = new HashSet<ProductHistoryGlobalCatMap>();
            this.ProductHistoryLocalCatMaps = new HashSet<ProductHistoryLocalCatMap>();
            this.ProductHistoryVariants = new HashSet<ProductHistoryVariant>();
        }
    
        public int ProductId { get; set; }
        public Nullable<int> GlobalCatId { get; set; }
        public Nullable<int> LocalCatId { get; set; }
        public Nullable<int> ShopId { get; set; }
        public Nullable<int> SellerId { get; set; }
        public Nullable<int> AttributeSetId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public Nullable<decimal> OriginalPrice { get; set; }
        public Nullable<decimal> SalePrice { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
        public Nullable<int> Stock { get; set; }
        public Nullable<int> SafetyStock { get; set; }
        public Nullable<int> ShippingId { get; set; }
        public string FeatureImgUrl { get; set; }
        public string Tag { get; set; }
        public Nullable<decimal> PrepareDay { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> Height { get; set; }
        public Nullable<decimal> Width { get; set; }
        public string DimensionUnit { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string WeightUnit { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKey { get; set; }
        public string UrlEn { get; set; }
        public string UrlTh { get; set; }
        public string BoostWeight { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<bool> ControlFlag1 { get; set; }
        public Nullable<bool> ControlFlag2 { get; set; }
        public Nullable<bool> ControlFlag3 { get; set; }
        public string Remark { get; set; }
        public Nullable<bool> InfoFlag { get; set; }
        public Nullable<bool> ImageFlag { get; set; }
        public Nullable<bool> OnlineFlag { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual AttributeSet AttributeSet { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual GlobalCategory GlobalCategory { get; set; }
        public virtual LocalCategory LocalCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryGlobalCatMap> ProductHistoryGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryLocalCatMap> ProductHistoryLocalCatMaps { get; set; }
        public virtual Shipping Shipping { get; set; }
        public virtual Shop Shop { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryVariant> ProductHistoryVariants { get; set; }
    }
}
