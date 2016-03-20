using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class PurchaseOrderReuest : PaginatedRequest
    {
        public DateTime OrderDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal GrandTotalAmt { get; set; }
        public string ShippingType { get; set; }
        public int ShopId { get; set; }
        public string ShipContactor { get; set; }
        public string ShipAddress1 { get; set; }
        public string ShipAddress2 { get; set; }
        public string ShipAddress3 { get; set; }
        public string ShipAddress4 { get; set; }
        public string ShipProvince { get; set; }
        public string ShipCity { get; set; }
        public string ShipDistrict { get; set; }
        public string ShipAreaCode { get; set; }
        public string BillAddress1 { get; set; }
        public string BillAddress2 { get; set; }
        public string BillAddress3 { get; set; }
        public string BillAddress4 { get; set; }
        public string InvoiceAddress1 { get; set; }
        public string InvoiceAddress2 { get; set; }
        public string InvoiceAddress3 { get; set; }
        public string InvoiceAddress4 { get; set; }

        public decimal OrdDiscAmt { get; set; }
        public List<PurchaseOrderDetailReuest> Products { get; set; }

        public string SearchText { get; set; }

        public PurchaseOrderReuest()
        {
            OrderId = string.Empty;
            CustomerName = string.Empty;
            Status = string.Empty;
            TotalAmt = 0;
            GrandTotalAmt = 0;
            Products = new List<PurchaseOrderDetailReuest>();
        }

        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "OrderDate");
            base.DefaultOnNull();
        }

    }
}
