using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class VariantRequest
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string ProdTDNameTh { get; set; }
        public string ProdTDNameEn { get; set; }
        public string IsHasExpiryDate { get; set; }
        public string IsVat { get; set; }
        public int MinimumAllowedInCart { get; set; }
        public int MaximumAllowedInCart { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public string DescriptionShortTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionFullEn { get; set; }
        public string MobileDescriptionEn { get; set; }
        public string MobileDescriptionTh { get; set; }

        public decimal OriginalPrice { get; set; }
        public int MaximumAllowedPreOrder { get; set; }
        public decimal SalePrice { get; set; }
        public int Quantity { get; set; }
        public int MaxQtyAllowInCart { get; set; }
        public int MinQtyAllowInCart { get; set; }
        public int MaxQtyPreOrder    { get; set; }
        public bool UseDecimal { get; set; }
        public decimal PromotionPrice { get; set; }
        public DateTime? PromotionEffectiveDate { get; set; }
        public DateTime? PromotionExpireDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public string SaleUnitTh { get; set; }
        public string SaleUnitEn { get; set; }
        public int PrepareDay { get; set; }
       
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
        public string DimensionUnit { get; set; }
        public string WeightUnit { get; set; }
        public string StockType { get; set; }
        public int SafetyStock { get; set; }
        public List<ImageRequest> Images { get; set; }
        public string Installment { get; set; }
        public string ExpressDelivery { get; set; }
        public decimal DeliveryFee { get; set; }
        public List<VideoLinkRequest> VideoLinks { get; set; }
        public SEORequest SEO { get; set; }
        public bool LimitIndividualDay { get; set; }
        public string Upc { get; set; }
        public AttributeRequest FirstAttribute { get; set; }
        public AttributeRequest SecondAttribute { get; set; }
        public string Display { get; set; }
        public bool Visibility { get; set; }
        public bool DefaultVariant { get; set; }
        
        public long ProductId { get; set; }
        public int ShopId { get; set; }
        
        public List<ImageRequest> Images360 { get; set; }
        
        public int VariantId { get; set; }
        public bool IsVariant { get; set; }
        public string Status { get; set; }
        public List<ImageRequest> MasterImg { get; set; }
        public List<ImageRequest> VariantImg { get; set; }


        public VariantRequest()
        {
            Length = 0;
            Width = 0;
            Height = 0;
            Weight = 0;
            ProductNameEn = string.Empty;
            Pid = string.Empty;
            ProductNameTh = string.Empty;
            ProdTDNameTh = string.Empty;
            ProdTDNameEn = string.Empty;
            Sku = string.Empty;
            DescriptionShortTh = string.Empty;
            DescriptionShortEn = string.Empty;
            DescriptionFullTh = string.Empty;
            DescriptionFullEn = string.Empty;
            OriginalPrice = 0;
            SalePrice = 0;
            Quantity = 0;
            PrepareDay = 0;
            LimitIndividualDay = false;
            PrepareMon = 0;
            PrepareTue = 0;
            PrepareWed = 0;
            PrepareThu = 0;
            PrepareFri = 0;
            PrepareSat = 0;
            PrepareSun = 0;
            KillerPoint1En = string.Empty;
            KillerPoint2En = string.Empty;
            KillerPoint3En = string.Empty;
            KillerPoint1Th = string.Empty;
            KillerPoint2Th = string.Empty;
            KillerPoint3Th = string.Empty;
            DimensionUnit = string.Empty;
            WeightUnit = string.Empty;
            StockType = string.Empty;
            Images = new List<ImageRequest>();
            Installment = string.Empty;
            //TheOneCardEarn = 0;
            //GiftWrap = string.Empty;
            //ShippingMethod = 0;
            VideoLinks = new List<VideoLinkRequest>();
            SEO = new SEORequest();
            FirstAttribute = new AttributeRequest();
            SecondAttribute = new AttributeRequest();
            //ValueEn = string.Empty;
            //ValueTh = string.Empty;
            Display = string.Empty;
            Visibility = false;
            IsVariant = false;
            ProductId = 0;
            VariantId = 0;
            DefaultVariant = false;
            VariantImg = new List<ImageRequest>();
            MasterImg = new List<ImageRequest>();
            IsHasExpiryDate = string.Empty;
            IsVat = string.Empty;
            MaxQtyAllowInCart = 0;
            MinQtyAllowInCart = 0;
            MaxQtyPreOrder = 0;
        }
    }
}
