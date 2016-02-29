using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    /// <summary>
    /// List of fields which can be exported
    /// This should be kept as a configuration
    /// </summary>
    public class ExportRequest
    {

        public List<string> Options { get; set; }

        //System Information
        public bool ProductStatus { get; set; }
        public bool PID { get; set; }
        public bool GroupID { get; set; }
        public bool GroupNameEn { get; set; }
        public bool GroupNameTh { get; set; }
        public bool DefaultVariant { get; set; }

        //Vitual Information
        public bool SKU { get; set; }
        public bool UPC { get; set; }
        public bool ProductNameEn { get; set; }
        public bool ProductNameTh { get; set; }
        public bool BrandName { get; set; }

        //Category
        public bool GlobalCategory { get; set; }
        public bool LocalCategory { get; set; }

        //Price
        public bool OriginalPrice { get; set; }
        public bool SalePrice { get; set; }

        //Description
        public bool DescriptionEn { get; set; }
        public bool DescriptionTh { get; set; }
        public bool ShortDescriptionEn { get; set; }
        public bool ShortDescriptionTh { get; set; }

        //Shipping and Inventory
        public bool StockType { get; set; }
        public bool ShippingMethod { get; set; }
        public bool PreparationTime { get; set; }
        public bool PackageLenght { get; set; }
        public bool PackageHeight { get; set; }
        public bool PackageWidth { get; set; }
        public bool PackageWeight { get; set; }
        public bool InventoryAmount { get; set; }
        public bool SafetytockAmount { get; set; }
        public bool SearchTag { get; set; }
        public bool RelatedProducts { get; set; }
        public bool MetaTitleEn { get; set; }
        public bool MetaTitleTh { get; set; }
        public bool MetaDescriptionEn { get; set; }
        public bool MetaDescriptionTh { get; set; }
        public bool MetaKeywordEn { get; set; }
        public bool MetaKeywordTh { get; set; }
        public bool ProductURLKeyEn { get; set; }
        public bool ProductBoostingWeight { get; set; }
        public bool EffectiveDate { get; set; }
        public bool EffectiveTime { get; set; }
        public bool ExpiryDate { get; set; }
        public bool ExpiryTime { get; set; }
        public bool Remark { get; set; }
        public bool AttributeSet { get; set; }
        public bool VariantOption01 { get; set; }
        public bool VariantOption02 { get; set; }
        public bool FlagControl1 { get; set; }
        public bool FlagControl2 { get; set; }
        public bool FlagControl3 { get; set; }

        //List of selected product to be exported
        public List<ProductStageRequest> ProductList { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }

        //Constructor
        public ExportRequest()
        {
            ProductList = new List<ProductStageRequest>();
            AttributeSets = new List<AttributeSetRequest>();
            Options = new List<string>();
        }
    }
}
