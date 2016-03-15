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
        public string Pid { get; set; }
        public string ParentId { get; set; }
        public int ShopId { get; set; }
        public int GlobalCatId { get; set; }
        public Nullable<int> LocalCatId { get; set; }
        public Nullable<int> AttributeSetId { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortTh { get; set; }
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
        public string GiftWarp { get; set; }
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
        public int BoostWeight { get; set; }
        public bool IsVariant { get; set; }
        public int VariantCount { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.DateTime> ExpireDate { get; set; }
        public bool ControlFlag1 { get; set; }
        public bool ControlFlag2 { get; set; }
        public bool ControlFlag3 { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual Brand Brand { get; set; }
        public virtual GlobalCategory GlobalCategory { get; set; }
        public virtual LocalCategory LocalCategory { get; set; }
        public virtual Shop Shop { get; set; }
    }
}
