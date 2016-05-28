using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ItemReturnReportRequest : PaginatedRequest
    {
        public string OrderId { get; set; }
        public string PID { get; set; }
        public string ItemName { get; set; }
        public string OrderDateFrom { get; set; }
        public string OrderDateTo { get; set; }
        public ItemReturnReportRequest()
        {

            OrderId = string.Empty;
            PID = string.Empty;
            ItemName = string.Empty;
            OrderDateFrom = string.Empty;
            OrderDateTo = string.Empty;
        }
    }
}
