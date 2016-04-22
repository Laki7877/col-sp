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




        public string VendorId { get; set; }
        public int ThemeId { get; set; }
        public string TaxPayerId { get; set; }
        public TermPaymentRequest TermPayment { get; set; }
        public string Payment { get; set; }
        public string VendorTaxRate { get; set; }
        public string WithholdingTax { get; set; }
        public string VendorAddressLine1 { get; set; }
        public string VendorAddressLine2 { get; set; }
        public string VendorAddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Telex { get; set; }
        public string OverseasVendorIndicator { get; set; }
        public string RemittanceFaxNumber { get; set; }

        public ShopRequest()
        {
            ShopId = 0;
            ThemeId = 0;

            VendorId = string.Empty;
            TaxPayerId = string.Empty;
            TermPayment = new TermPaymentRequest();
            Payment = string.Empty;
            VendorTaxRate = string.Empty;
            WithholdingTax = string.Empty;
            VendorAddressLine1 = string.Empty;
            VendorAddressLine2 = string.Empty;
            VendorAddressLine3 = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
            CountryCode = string.Empty;
            Country = string.Empty;
            PhoneNumber = string.Empty;
            FaxNumber = string.Empty;
            Telex = string.Empty;
            OverseasVendorIndicator = string.Empty;
            RemittanceFaxNumber = string.Empty;



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

    public class TermPaymentRequest
    {
        public string TermCode { get; set; }
        public string Description { get; set; }

        public TermPaymentRequest()
        {
            TermCode = string.Empty;
            Description = string.Empty;
        }
    }



}
