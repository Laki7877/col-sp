using System;
using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class PurchaseOrderReuest : PaginatedRequest
    {
        public DateTime OrderDate { get; set; }
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal GrandTotalAmt { get; set; }
        public string ShippingType { get; set; }
        public int ShopId { get; set; }
        public string ShipAddress { get; set; }
        public string BillAddress { get; set; }
        public decimal OrdDiscAmt { get; set; }
        public List<PurchaseOrderDetailReuest> Products { get; set; }

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
