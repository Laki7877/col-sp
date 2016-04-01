using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ShopRequest : PaginatedRequest
    {
        public int ShopId { get; set; }
        public string ShopNameEn { get; set; }
        public string ShopNameTh { get; set; }
        public string UrlKeyEn { get; set; }
        public string Status { get; set; }
        public UserRequest ShopOwner { get; set; }
        public List<UserRequest> Users { get; set; }
        public ShopTypeRequest ShopType { get; set; }
        public decimal Commission { get; set; }
        public string ShopDescriptionEn { get; set; }
        public string ShopDescriptionTh { get; set; }
        public string FloatMessageEn { get; set; }
        public string FloatMessageTh { get; set; }
        public string ShopAddress { get; set; }
        public int MaxLocalCategory { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string Facebook { get; set; }
        public string YouTube { get; set; }
        public string Instagram { get; set; }
        public string Pinterest { get; set; }
        public string Twitter { get; set; }
        public int StockAlert { get; set; }
        public ImageRequest ShopImage { get; set; }
        public string SearchText { get; set; }
        public string ShopGroup { get; set; }
        public string TaxInvoice { get; set; }
        public string GiftWrap { get; set; }
        //public bool IsShopReady { get; set; }
        public List<ShopCommission> Commissions { get; set; }


        public ShopRequest()
        {
            ShopId = 0;
            ShopNameEn = string.Empty;
            ShopNameTh = string.Empty;
            Status = string.Empty;
            Commission = 0;
            ShopDescriptionEn = string.Empty;
            ShopDescriptionTh = string.Empty;
            FloatMessageEn = string.Empty;
            FloatMessageTh = string.Empty;
            ShopAddress = string.Empty;
            MaxLocalCategory = 0;
            BankName = string.Empty;
            BankAccountName = string.Empty;
            BankAccountNumber = string.Empty;
            Facebook = string.Empty;
            YouTube = string.Empty;
            Instagram = string.Empty;
            Pinterest = string.Empty;
            Twitter = string.Empty;
            StockAlert = 0;
            SearchText = string.Empty;
            ShopGroup = string.Empty;
            TaxInvoice = string.Empty;
            GiftWrap = string.Empty;

            ShopOwner = new UserRequest();
            Commissions = new List<ShopCommission>();
            Users = new List<UserRequest>();
            ShopType = new ShopTypeRequest();
            ShopImage = new ImageRequest();
        }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "ShopId");
            base.DefaultOnNull();
        }

        
    }

    
}
