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
        public BankDetailRequest BankName { get; set; }
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
        public VendorTaxRateRequest VendorTaxRate { get; set; }
        public WithholdingTaxRequest WithholdingTax { get; set; }
        public string VendorAddressLine1 { get; set; }
        public string VendorAddressLine2 { get; set; }
        public string VendorAddressLine3 { get; set; }
        public CityRequest City { get; set; }
        public ProvinceRequest Province { get; set; }
        public DistrictRequest District { get; set; }
        public CountryRequest Country { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
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
            VendorTaxRate = new VendorTaxRateRequest();
            WithholdingTax = new WithholdingTaxRequest();
            VendorAddressLine1 = string.Empty;
            VendorAddressLine2 = string.Empty;
            VendorAddressLine3 = string.Empty;
            Country = new CountryRequest();
            Province = new ProvinceRequest();
            City = new CityRequest();
            District = new DistrictRequest();
            State = string.Empty;
            ZipCode = string.Empty;
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
            BankName = new BankDetailRequest();
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
        public string TermPaymentCode { get; set; }
        public string Description { get; set; }

        public TermPaymentRequest()
        {
            TermPaymentCode = string.Empty;
            Description = string.Empty;
        }
    }

    public class WithholdingTaxRequest
    {
        public string Description { get; set; }
        public string WithholdingTaxCode { get; set; }

        public WithholdingTaxRequest()
        {
            Description = string.Empty;
            WithholdingTaxCode = string.Empty;
        }
    }

    public class VendorTaxRateRequest
    {
        public string Description { get; set; }
        public string VendorTaxRateCode { get; set; }

        public VendorTaxRateRequest()
        {
            Description = string.Empty;
            VendorTaxRateCode = string.Empty;
        }
    }


    public class BankDetailRequest
    {
        public string BankName { get; set; }
        public string BankNumber { get; set; }
        
        public BankDetailRequest()
        {
            BankName = string.Empty;
            BankNumber = string.Empty;
        }
    }


    public class CityRequest
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CityNameEn { get; set; }

        public CityRequest()
        {
            CityId = 0;
            CityName = string.Empty;
            CityNameEn = string.Empty;
        }
    }

    public class ProvinceRequest
    {
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceNameEn { get; set; }

        public ProvinceRequest()
        {
            ProvinceId = 0;
            ProvinceName = string.Empty;
            ProvinceNameEn = string.Empty;
        }
    }


    public class CountryRequest
    {
        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        public CountryRequest()
        {
            CountryCode = string.Empty;
            CountryName = string.Empty;
        }
    }

    public class DistrictRequest
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string DistrictNameEn { get; set; }

        public DistrictRequest()
        {
            DistrictId = 0;
            DistrictName = string.Empty;
            DistrictNameEn = string.Empty;
        }
    }


    
}
