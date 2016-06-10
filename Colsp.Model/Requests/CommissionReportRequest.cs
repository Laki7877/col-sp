using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CommissionReportRequest : PaginatedRequest
    {
        public string OrderId { get; set; }
        public string PID { get; set; }
        public string ProductName { get; set; }
        public string OrderDateFrom { get; set; }
        public string OrderDateEnd { get; set; }
        public CommissionReportRequest() {
            OrderId = string.Empty;
            PID = string.Empty;
            ProductName = string.Empty;
        }
    }
}
