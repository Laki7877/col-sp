using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CmosProductRequest
    {
        public string ProductID;
        public string NameThai;
        public string NameEng;
        public string DocumentNameThai;
        public string DocumentNameEng;
        public string StockType;
        public string ProductStatus;
        public string Remark;
        public string DescriptionShortThai;
        public string DescriptionShotEng;
        public string DescriptionFullThai;
        public string DescriptionFullEng;
        public string Upc;
        public string SaleUnitThai;
        public string SaleUnitEng;
        public string JDADept;
        public string JDASubDept;
        public int BrandId;
        public string BrandNameThai;
        public string BrandNameEng;
        public int PrepareDay;
        public decimal Width;
        public decimal Length;
        public decimal Height;
        public decimal Weight;
        public string Sku;
        public bool IsHasExpiryDate;
        public decimal OriginalPrice;
        public decimal SalePrice;
        public decimal DeliverFee;
        public bool IsVat;
        public bool IsBestDeal;
        public int ShopId;

        public CmosProductRequest()
        {
            ProductID = string.Empty;
            NameThai = string.Empty;
            NameEng = string.Empty;
            DocumentNameThai = string.Empty;
            DocumentNameEng = string.Empty;
            StockType = string.Empty;
            ProductStatus = string.Empty;
            Remark = string.Empty;
            DescriptionShortThai = string.Empty;
            DescriptionShotEng = string.Empty;
            DescriptionFullThai = string.Empty;
            DescriptionFullEng = string.Empty;
            Upc = string.Empty;
            SaleUnitThai = string.Empty;
            SaleUnitEng = string.Empty;
            JDADept = string.Empty;
            JDASubDept = string.Empty;
            BrandNameThai = string.Empty;
            BrandNameEng = string.Empty;
            Sku = string.Empty;
            IsHasExpiryDate = false;
            IsVat = false;
            IsBestDeal = false;
            BrandId = 0;
            PrepareDay = 0;
            Width = 0;
            Length = 0;
            Height = 0;
            Weight = 0;
            OriginalPrice = 0;
            SalePrice = 0;
            DeliverFee = 0;
            ShopId = 0;


        }
    }
}
