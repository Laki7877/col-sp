using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class StockStatusReportRequest : PaginatedRequest
    {
        public string Pid { get; set; }
        public string ProductName { get; set; }
        public string variant { get; set; }
        public string LastSoldDate { get; set; }
        public override void DefaultOnNull()
        {
            Pid = string.Empty;
            ProductName = string.Empty;
            variant = string.Empty;
            LastSoldDate = string.Empty;

        }
    }
}
