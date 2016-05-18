using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ProductStageRequest
    {
        public long ProductId { get; set; }
        public VariantRequest MasterVariant { get; set; }
        public List<VariantRequest> Variants { get; set; }
        public string Status { get; set; }
        public int ShopId { get; set; }
        public bool Visibility { get; set; }
        public List<CategoryRequest> GlobalCategories { get; set; }
        public List<CategoryRequest> LocalCategories { get; set; }
        public CategoryRequest MainGlobalCategory { get; set; }
        public CategoryRequest MainLocalCategory { get; set; }
        public List<string> Tags { get; set; }
        public ControlFlagRequest ControlFlags { get; set; }
        public BrandRequest Brand { get; set; }
        public int TheOneCardEarn { get; set; }
        public string GiftWrap { get; set; }
        public AttributeSetRequest AttributeSet { get; set; }
        public List<AttributeRequest> MasterAttribute { get; set; }
        public List<VariantRequest> RelatedProducts { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string Remark { get; set; }
        public bool ImageFlag { get; set; }
        public bool InfoFlag { get; set; }
        public bool OnlineFlag { get; set; }
        public int ShippingMethod { get; set; }
        public AdminApproveRequest AdminApprove { get; set; }
        public int SellerId { get; set; }
        public int VariantCount { get; set; }
        public string GroupId { get; set; }
        public string Pid { get; set; }
        public List<ProductHistoryRequest> Revisions { get; set; }
        public DateTime? NewArrivalDate { get; set; }

        public ProductStageRequest()
        {
            MasterVariant = new VariantRequest();
            Variants = new List<VariantRequest>();
            Status = string.Empty;
            ShopId = 0;
            Visibility = false;
            MainGlobalCategory = new CategoryRequest();
            GlobalCategories = new List<CategoryRequest>();
            MainLocalCategory = new CategoryRequest();
            LocalCategories = new List<CategoryRequest>();
            Tags = new List<string>();
            Brand = new BrandRequest();
            TheOneCardEarn = 0;
            GiftWrap = string.Empty;
            AttributeSet = new AttributeSetRequest();
            MasterAttribute = new List<AttributeRequest>();
            RelatedProducts = new List<VariantRequest>();
            ShippingMethod = 1;
            EffectiveDate = null;
            ExpireDate = null;
            Remark = string.Empty;
            SellerId = 0;
            ProductId = 0;
            InfoFlag = false;
            ImageFlag = false;
            OnlineFlag = false;
            VariantCount = 0;
            ControlFlags = new ControlFlagRequest();
            GroupId = string.Empty;
            AdminApprove = new AdminApproveRequest();
            Revisions = new List<ProductHistoryRequest>();
        }
    }

}
