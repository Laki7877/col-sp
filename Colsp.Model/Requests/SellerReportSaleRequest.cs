using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class SellerReportSaleRequest : PaginatedRequest
    {
        public DateTime OrderDateFrom { get; set; }
        public DateTime OrderDateEnd { get; set; }
        public string ItemStatus { get; set; }
        public int PID { get; set; }
        public string ItemName { get; set; }
        public int BrandId { get; set; }
        public int GlobalCategoryId { get; set; }
        public int LocalCategoryId { get; set; }
        public SellerReportSaleRequest()
        {
            OrderDateFrom = new DateTime();
            OrderDateEnd = new DateTime();
            ItemStatus = string.Empty;
            PID = 0;
            ItemName = string.Empty;
            BrandId = 0;
            GlobalCategoryId = 0;
            LocalCategoryId = 0;
        }
    }
}

   
