using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class CmosShopRequest
	{
		public int ShopId { get; set; }
		public string VendorId { get; set; }
		public int? ShopOwner { get; set; }
		public string ShopNameEn { get; set; }
		public string ShopNameTh { get; set; }
		public int MaxLocalCategory { get; set; }
		public string GiftWrap { get; set; }
		public string TaxInvoice { get; set; }
		public string ShopGroup { get; set; }
		public string ShopImageUrl { get; set; }
		public string ShopDescriptionEn { get; set; }
		public string ShopDescriptionTh { get; set; }
		public string DomainName { get; set; }
		public string ShopAddress { get; set; }
		public string Facebook { get; set; }
		public string YouTube { get; set; }
		public string Twitter { get; set; }
		public string Instagram { get; set; }
		public string Pinterest { get; set; }
		public int StockAlert { get; set; }
		public decimal Commission { get; set; }
		public int? ShopTypeId { get; set; }
		public string UrlKey { get; set; }
		public string FloatMessageEn { get; set; }
		public string FloatMessageTh { get; set; }
		public int? ThemeId { get; set; }
		public string ShopAppearance { get; set; }
		public string TaxPayerId { get; set; }
		public string TermPaymentCode { get; set; }
		public string Payment { get; set; }
		public string VendorTaxRate { get; set; }
		public string WithholdingTaxCode { get; set; }
		public string VendorAddressLine1 { get; set; }
		public string VendorAddressLine2 { get; set; }
		public string VendorAddressLine3 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public int? DistrictId { get; set; }
		public string ZipCode { get; set; }
		public string CountryCode { get; set; }
		public string PhoneNumber { get; set; }
		public string FaxNumber { get; set; }
		public string Telex { get; set; }
		public string OverseasVendorIndicator { get; set; }
		public string BankNumber { get; set; }
		public string BankAccountNumber { get; set; }
		public string BankAccountName { get; set; }
		public string RemittanceFaxNumber { get; set; }
		public string ContactPersonFirstName { get; set; }
		public string ContactPersonLastName { get; set; }
		public string Email { get; set; }
		public string Status { get; set; }
		public string CreateBy { get; set; }
		public DateTime? CreateDt { get; set; }
		public string UpdateBy { get; set; }
		public DateTime? UpdateDt { get; set; }

	}
}
