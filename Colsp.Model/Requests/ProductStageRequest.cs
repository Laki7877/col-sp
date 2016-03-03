﻿using System;
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
        public SEORequest SEO { get; set; }
        public BrandRequest Brand { get; set; }
        public int? ShippingMethod { get; set; }
        public decimal? PrepareDay { get; set; }
        public List<VariantRequest> RelatedProducts { get; set; }
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
        public ControlFlag ControlFlags { get; set; }
        public string GroupId { get; set; }


        public ProductStageRequest()
        {
            AttributeSet = new AttributeSetRequest();
            GlobalCategories = new List<CategoryRequest>();
            LocalCategories = new List<CategoryRequest>();
            MasterVariant = new VariantRequest();
            MasterAttribute = new List<AttributeRequest>();
            Variants = new List<VariantRequest>();
            SEO = new SEORequest();
            Brand = new BrandRequest();
            RelatedProducts = new List<VariantRequest>();
            ControlFlags = new ControlFlag();
        }

    }

    public class VariantRequest
    {
        public int? ProductId { get; set; }
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
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionShortTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public int? Quantity { get; set; }
        public int? SafetyStock { get; set; }
        public string StockType { get; set; }
        public bool? DefaultVariant { get; set; }
        public int? VariantId { get; set; }
        public bool? Visibility { get; set; }
        public bool? IsVariant { get; set; }
        public SEORequest SEO { get; set; }
        public List<ImageRequest> MasterImg { get; set; }
        public List<ImageRequest> VariantImg { get; set; }
        public decimal? PrepareDay { get; set; }

        public VariantRequest()
        {
            SEO = new SEORequest();
            FirstAttribute = new AttributeRequest();
            SecondAttribute = new AttributeRequest();
            Images = new List<ImageRequest>();
            VideoLinks = new List<VideoLinkRequest>();
            VariantImg = new List<ImageRequest>();
            MasterImg = new List<ImageRequest>();
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
    
    public class ControlFlag
    {
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
    }


}
