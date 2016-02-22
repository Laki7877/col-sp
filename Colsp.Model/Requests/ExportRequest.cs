using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ExportRequest
    {
        /*List of fields which can be exported
         *This should be kept as a configuration*/
        //System Information
        public bool ProductStatus { get; set; }
        public bool PID { get; set; }
        public bool GroupID { get; set; }

        //Vitual Information
        public bool SKU { get; set; }
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
        public bool? StockType { get; set; }
        public bool? PreparationTime { get; set; }
        public bool? PackageLenght { get; set; }
        public bool? PackageHeight { get; set; }
        public bool? PackageWidth { get; set; }
        public bool? PackageWeight { get; set; }
        public bool? InventoryAmount { get; set; }
        public bool? SafetytockAmount { get; set; }


        //List of selected product to be exported
        public List<ProductStageRequest> ProductList { get; set; }

        //Constructor
        public ExportRequest()
        {
            //System Information
            ProductStatus = false;
            PID = false;
            GroupID = false;

            //Vitual Information
            SKU = false;
            ProductNameEn = false;
            ProductNameTh = false;
            BrandName = false;

            //Category
            GlobalCategory = false;
            LocalCategory = false;

            //Price
            OriginalPrice = false;
            SalePrice = false;

            //Description
            DescriptionEn = false;
            DescriptionTh = false;
            ShortDescriptionEn = false;
            ShortDescriptionTh = false;

            //Shipping and Inventory
            StockType = false;
            PreparationTime = false;
            PackageLenght = false;
            PackageHeight = false;
            PackageWidth = false;
            PackageWeight = false;
            InventoryAmount = false;
            SafetytockAmount = false;

            ProductList = new List<ProductStageRequest>();
        }
    }
}
