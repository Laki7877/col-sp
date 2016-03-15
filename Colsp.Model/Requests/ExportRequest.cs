using System.Collections.Generic;

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
        public bool ProductStatus           { get; set; }
        public bool PID                     { get; set; }
        public bool GroupID                 { get; set; }
        public bool GroupNameEn             { get; set; }
        public bool GroupNameTh             { get; set; }
        public bool DefaultVariant          { get; set; }

        //Vitual Information
        public bool SKU                     { get; set; }
        public bool UPC                     { get; set; }
        public bool ProductNameEn           { get; set; }
        public bool ProductNameTh           { get; set; }
        public bool BrandName               { get; set; }

        //Category
        public bool GlobalCategory          { get; set; }
        public bool GlobalCategory01        { get; set; }
        public bool GlobalCategory02        { get; set; }
        public bool LocalCategory           { get; set; }
        public bool LocalCategory01         { get; set; }
        public bool LocalCategory02         { get; set; }

        //Price
        public bool OriginalPrice           { get; set; }
        public bool SalePrice               { get; set; }

        //Description
        public bool DescriptionEn           { get; set; }
        public bool DescriptionTh           { get; set; }
        public bool ShortDescriptionEn      { get; set; }
        public bool ShortDescriptionTh      { get; set; }

        //Shipping and Inventory
        public bool StockType               { get; set; }
        public bool ShippingMethod          { get; set; }
        public bool PreparationTime         { get; set; }
        public bool PackageLenght           { get; set; }
        public bool PackageHeight           { get; set; }
        public bool PackageWidth            { get; set; }
        public bool PackageWeight           { get; set; }
        public bool InventoryAmount         { get; set; }
        public bool SafetytockAmount        { get; set; }
        public bool SearchTag               { get; set; }
        public bool RelatedProducts         { get; set; }
        public bool MetaTitleEn             { get; set; }
        public bool MetaTitleTh             { get; set; }
        public bool MetaDescriptionEn       { get; set; }
        public bool MetaDescriptionTh       { get; set; }
        public bool MetaKeywordEn           { get; set; }
        public bool MetaKeywordTh           { get; set; }
        public bool ProductURLKeyEn         { get; set; }
        public bool ProductBoostingWeight   { get; set; }
        public bool EffectiveDate           { get; set; }
        public bool EffectiveTime           { get; set; }
        public bool ExpiryDate              { get; set; }
        public bool ExpiryTime              { get; set; }
        public bool Remark                  { get; set; }
        public bool AttributeSet            { get; set; }
        public bool VariantOption01         { get; set; }
        public bool VariantOption02         { get; set; }
        public bool FlagControl1            { get; set; }
        public bool FlagControl2            { get; set; }
        public bool FlagControl3            { get; set; }

        //List of selected product to be exported
        public List<ProductStageRequest> ProductList { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }

        //Constructor
        public ExportRequest()
        {
            ProductStatus             = false;
            PID                       = false;
            GroupID                   = false;
            GroupNameEn               = false;
            GroupNameTh               = false;
            DefaultVariant            = false;
            SKU                       = false;
            UPC                       = false;
            ProductNameEn             = false;
            ProductNameTh             = false;
            BrandName                 = false;
            GlobalCategory            = false;
            GlobalCategory01          = false;
            GlobalCategory02          = false;
            LocalCategory             = false;
            LocalCategory01           = false;
            LocalCategory02           = false;
            OriginalPrice             = false;
            SalePrice                 = false;
            DescriptionEn             = false;
            DescriptionTh             = false;
            ShortDescriptionEn        = false;
            ShortDescriptionTh        = false;
            StockType                 = false;
            ShippingMethod            = false;
            PreparationTime           = false;
            PackageLenght             = false;
            PackageHeight             = false;
            PackageWidth              = false;
            PackageWeight             = false;
            InventoryAmount           = false;
            SafetytockAmount          = false;
            SearchTag                 = false;
            RelatedProducts           = false;
            MetaTitleEn               = false;
            MetaTitleTh               = false;
            MetaDescriptionEn         = false;
            MetaDescriptionTh         = false;
            MetaKeywordEn             = false;
            MetaKeywordTh             = false;
            ProductURLKeyEn           = false;
            ProductBoostingWeight     = false;
            EffectiveDate             = false;
            EffectiveTime             = false;
            ExpiryDate                = false;
            ExpiryTime                = false;
            Remark                    = false;
            AttributeSet              = false;
            VariantOption01           = false;
            VariantOption02           = false;
            FlagControl1              = false;
            FlagControl2              = false;
            FlagControl3              = false;      
            ProductList = new List<ProductStageRequest>();
            AttributeSets = new List<AttributeSetRequest>();
            Options = new List<string>();
        }
    }
}
