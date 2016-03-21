using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ReturnRequest : PaginatedRequest
    {
        public DateTime ReturnDate { get; set; }
        public string ReturnId { get; set; }
        public PurchaseOrderReuest Order { get; set; }
        public int ShopId { get; set; }
        public string CnNumber { get; set; }
        public string ReasonForReturn { get; set; }
        public string Status { get; set; }
        public string SearchText { get; set; }
        //public string OrderId { get; set; }
        //public string CustomerName { get; set; }
        //public string Status { get; set; }
        //public decimal TotalAmt { get; set; }
        //public decimal GrandTotalAmt { get; set; }
        //public int ShopId { get; set; }
        //public decimal OrdDiscAmt { get; set; }
        //public List<PurchaseOrderDetailReuest> Products { get; set; }

        public override void DefaultOnNull()
        {
            _order = GetValueOrDefault(_order, "ReturnDate");
            base.DefaultOnNull();
        }
    }
}
