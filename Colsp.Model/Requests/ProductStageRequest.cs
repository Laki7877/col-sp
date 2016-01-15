using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ProductStageRequest
    {
        public string Keywords { get; set; }
        public AttributeSetRequest AttributeSet { get; set; }
        public int? GlobalCategory { get; set; }
        public List<CategoryRequest> GlobalCategories { get; set; }
        public int? LocalCategory { get; set; }
        public List<CategoryRequest> LocalCategories { get; set; }
        public VariantRequest MasterVariant { get; set; }
        public List<AttributeRequest> MasterAttribute { get; set; }
        public List<VariantRequest> Variants { get; set; }
        //public List<ImageRequest> MasterImages { get; set; }
        //public List<ImageRequest> MasterImages360 { get; set; }
        //public List<VideoLinkRequest> VideoLinks { get; set; }
        public SEORequest SEO { get; set; }
        //public string ProductNameTh { get; set; }
        //public string ProductNameEn { get; set; }
        //public string Sku { get; set; }
        //public string Upc { get; set; }
        public BrandRequest Brand { get; set; }
        //public decimal? OriginalPrice { get; set; }
        //public decimal? SalePrice { get; set; }
        //public string DescriptionFullTh { get; set; }
        //public string DescriptionFullEn { get; set; }
        //public string DescriptionShortTh { get; set; }
        //public string DescriptionShortEn { get; set; }
        //public int? Quantity { get; set; }
        //public int? SafetyStock { get; set; }
        //public string StockType { get; set; }
        public int? ShippingMethod { get; set; }
        public decimal? PrepareDay { get; set; }
        //public decimal? Length { get; set; }
        //public decimal? Height { get; set; }
        //public decimal? Width { get; set; }
        //public string DimensionUnit { get; set; }
        //public decimal? Weight { get; set; }
        //public string WeightUnit { get; set; }
        public List<string> RelatedProducts { get; set; }
        public string EffectiveDate { get; set; }
        public string EffectiveTime { get; set; }
        public string ExpireDate { get; set; }
        public string ExpireTime { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public int? SellerId { get; set; }
        public int? ShopId { get; set; }
        public int? ProductId { get; set; }
        public bool? InfoFlag { get; set; }
        public bool? ImageFlag { get; set; }
        public bool? OnlineFlag { get; set; }
        public bool? Visibility { get; set; }
        public int? VariantCount { get; set; }

        public ProductStageRequest()
        {
            AttributeSet = new AttributeSetRequest();
            MasterVariant = new VariantRequest();
            MasterAttribute = new List<AttributeRequest>();
            Variants = new List<VariantRequest>();
            SEO = new SEORequest();
            Brand = new BrandRequest();
        }

    }

    public class VariantRequest
    {
        public string ProductNameTh { get; set; }
        public string ProductNameEn { get; set; }
        public string Pid { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public AttributeRequest FirstAttribute { get; set; }
        public AttributeRequest SecondAttribute { get; set; }
        public string hash { get; set; }
        public string text { get; set; }
        public List<ImageRequest> Images { get; set; }
        public List<ImageRequest> Images360 { get; set; }
        public string ValueEn { get; set; }
        public string ValueTh { get; set; }
        public string Display { get; set; }
        public List<VideoLinkRequest> VideoLinks { get; set; }
        public decimal? Length { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public string DimensionUnit { get; set; }
        public decimal? Weight { get; set; }
        public string WeightUnit { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public int? Quantity { get; set; }
        public int? SafetyStock { get; set; }
        public string StockType { get; set; }
        public bool? DefaultVariant { get; set; }
        public int? VariantId { get; set; }

        public VariantRequest()
        {
            FirstAttribute = new AttributeRequest();
            SecondAttribute = new AttributeRequest();
            Images = new List<ImageRequest>();
            VideoLinks = new List<VideoLinkRequest>();
        }
    }
    
    
    public class ImageRequest
    {
        public string tmpPath { get; set; }
        public string url { get; set; }
        public int? position { get; set; }
        public string ImageName { get; set; }
        public int? ImageId;
    }
    public class VideoLinkRequest
    {
        public string Url { get; set; }
        public int? VideoId { get; set; }
    }
    
    
}
