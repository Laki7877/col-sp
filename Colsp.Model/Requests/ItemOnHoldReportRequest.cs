using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ItemOnHoldReportRequest : PaginatedRequest
    {
        public string PID { get; set; }
        public string ItemName { get; set; }
        public string OrderDateFrom { get; set; }
        public string OrderDateEnd { get; set; }
        public ItemOnHoldReportRequest()
        {
            OrderDateFrom = string.Empty;
            OrderDateEnd = string.Empty;
            PID = string.Empty;
            ItemName = string.Empty;
        }
        public override void DefaultOnNull()
        {
            ItemName = GetValueOrDefault(ItemName, string.Empty);
            OrderDateFrom = GetValueOrDefault(OrderDateFrom, string.Empty);
            OrderDateEnd = GetValueOrDefault(OrderDateEnd, string.Empty);
            PID = GetValueOrDefault(PID, string.Empty);
        }
    }
}
