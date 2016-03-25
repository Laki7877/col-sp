using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class PurchaseOrderDetailReuest
    {
        public string Pid { get; set; }
        public string ProductNameEn { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int ShipQuantity { get; set; }
        public int ProductId { get; set; }

    }
}
