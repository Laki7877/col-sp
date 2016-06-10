using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
	public class OrderResponse
	{
		public List<SubOrderResponse> suborders { get; set; }
		public string returncode { get; set; }
		public Dictionary<string,string> message { get; set; }
	}

	public class SubOrderResponse
	{
		public string SubOrderId { get; set; }
		public string OrderId { get; set; }
		public string CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string OrderGuid { get; set; }
		public string ShopId { get; set; }
		public string ShopName { get; set; }
		public string ShopNameEn { get; set; }
		public string Email { get; set; }
		public int BillAddressId { get; set; }
		public string BillAddress1 { get; set; }
		public string BillAddress2 { get; set; }
		public string BillAddress3 { get; set; }
		public string BillAddress4 { get; set; }
		public string BillBranch { get; set; }
		public string BillTaxId { get; set; }
		public string TrackingId { get; set; }
		public string InvNo { get; set; }
		public int ShipAddressId { get; set; }
		public string ShipContactor { get; set; }
		public string ShipPhoneNo { get; set; }
		public string ShipMobileNo { get; set; }
		public string ShipAddr1 { get; set; }
		public string ShipAddr2 { get; set; }
		public string ShipAddr3 { get; set; }
		public string ShipAddr4 { get; set; }
		public string ShipProvince { get; set; }
		public string ShipCity { get; set; }
		public string ShipDistrict { get; set; }
		public string ShipPostCode { get; set; }
		public int ShipAreaCode { get; set; }
		public string BranchCode { get; set; }
		public string BranchType { get; set; }
		public string NetAmtDisplay { get; set; }
		public decimal NetAmt { get; set; }
		public string VatAmtDisplay { get; set; }
		public decimal VatAmt { get; set; }
		public string VatProdNetAmtDisplay { get; set; }
		public decimal VatProdNetAmt { get; set; }
		public string NonVatProdNetAmtDisplay { get; set; }
		public decimal NonVatProdNetAmt { get; set; }
		public string TotalAmtDisplay { get; set; }
		public decimal TotalAmt { get; set; }
		public string GrandTotalAmtDisplay { get; set; }
		public decimal GrandTotalAmt { get; set; }
		public string VatRateDisplay { get; set; }
		public decimal VatRate { get; set; }
		public string ItemDeliverFeeDisplay { get; set; }
		public decimal ItemDeliverFee { get; set; }
		public string OrdDeliverFeeDisplay { get; set; }
		public decimal OrdDeliverFee { get; set; }
		public string AdditionalAmtDisplay { get; set; }
		public decimal AdditionalAmt { get; set; }
		public string ItemDiscAmtDisplay { get; set; }
		public decimal ItemDiscAmt { get; set; }
		public string OrdDiscAmtDisplay { get; set; }
		public decimal OrdDiscAmt { get; set; }
		public decimal CashVoucherDisplay { get; set; }
		public decimal CashVoucher { get; set; }
		public string RedeemRateDisplay { get; set; }
		public decimal RedeemRate { get; set; }
		public int RedeemPoint { get; set; }
		public string RedeemCashDisplay { get; set; }
		public decimal RedeemCash { get; set; }
		public string RedeemAmtDisplay { get; set; }
		public decimal RedeemAmt { get; set; }
		public bool IsSmsReceive { get; set; }
		public int Status { get; set; } //https://devmkp-api.cenergy.co.th/Help/ResourceModel?modelName=SubOrderStatus
		public int OrderType { get; set; } //https://devmkp-api.cenergy.co.th/Help/ResourceModel?modelName=OrderType
		public int PaymentId { get; set; }
		public string PaymentCode { get; set; }
		public string PaymentType { get; set; }
		public string T1CNoEarn { get; set; }
		public string T1CNoBurn { get; set; }
		public string PaymentRefNo { get; set; }
		public string T1CRefNo { get; set; }
		public string OrderRemark { get; set; }
		public string ProcessRemark { get; set; }
		public string ShippingRemark { get; set; }
		public string StatusRemark { get; set; }
		public string GiftMessage { get; set; }
		public string PromotionDesc { get; set; }
		public string Locale { get; set; }
		public DateTime? PaymentDate { get; set; }
		public DateTime? OrderDate { get; set; }
		public DateTime? ConfirmDeliveryDate { get; set; }
		public int SaleChannel { get; set; }
		public string PromotionDiscountCode { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public string MobileNoReceiveCode { get; set; }
		public string IPPInterestType { get; set; }
		public bool IsGiftForFriend { get; set; }
		public string DownloadStatus { get; set; }
		public bool IsDownload { get; set; }
		public string DownloadBy { get; set; }
		public DateTime? DownloadOn { get; set; }
		public int ShippingId { get; set; }
		public DateTime? CreateOn { get; set; }
		public string CreateBy { get; set; }
		public DateTime? UpdateOn { get; set; }
		public string UpdateBy { get; set; }
		public decimal OldNetAmt { get; set; }
		public decimal OldVatAmt { get; set; }
		public decimal OldNonVatProdNetAmt { get; set; }
		public decimal OldVatProdNetAmt { get; set; }
		public decimal OldTotalAmt { get; set; }
		public decimal OldGrandTotalAmt { get; set; }
		public decimal OldOrdDiscAmt { get; set; }
		public decimal OldItemDiscAmt { get; set; }
		public string LastStatus { get; set; }
		public string GiftWrapRemark { get; set; }
		public List<OrderDetailResponse> Details { get; set; }

	}

	public class OrderDetailResponse
	{
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal OrdDiscAmt { get; set; }
		public decimal ItemDiscAmt { get; set; }
		public bool IsBestDeal { get; set; }
		public bool IsVat { get; set; }
		public decimal ItemDeliverFee { get; set; }

	}
}
