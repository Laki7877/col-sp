using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ReturnReportRequest : PaginatedRequest
    {
        public string OrderId { get; set; }
        public string PID { get; set; }
        public string ItemName { get; set; }
        public string ItemStatus { get; set; }
        public string ReturnDateFrom { get; set; }
        public string ReturnDateTo { get; set; }
        public ReturnReportRequest()
        {
            OrderId = string.Empty;
            PID = string.Empty;
            ItemName = string.Empty;
            ItemStatus = string.Empty;
            ReturnDateFrom = string.Empty;
            ReturnDateTo = string.Empty;
        }
    }
}
