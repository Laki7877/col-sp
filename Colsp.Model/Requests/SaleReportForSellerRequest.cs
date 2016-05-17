using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class SaleReportForSellerRequest : PaginatedRequest
    {
        public string OrderDateFrom { get; set; }
        public string OrderDateEnd { get; set; }
        public string ItemStatus { get; set; }
        public int? PID { get; set; }
        public string ItemName { get; set; }
        public int? BrandId { get; set; }
        public int? GlobalCategoryId { get; set; }
        public int? LocalCategoryId { get; set; }
        public SaleReportForSellerRequest()
        {
            OrderDateFrom = string.Empty;
            OrderDateEnd = string.Empty;
            ItemStatus = string.Empty;
            PID = 0;
            ItemName = string.Empty;
            BrandId = 0;
            GlobalCategoryId = 0;
            LocalCategoryId = 0;
        }

        public override void DefaultOnNull()
        {
            LocalCategoryId = GetValueOrDefault(LocalCategoryId, 0);
            GlobalCategoryId = GetValueOrDefault(GlobalCategoryId, 0);
            BrandId = GetValueOrDefault(BrandId, 0);
            ItemStatus = GetValueOrDefault(ItemStatus, string.Empty);
            PID = GetValueOrDefault(PID, null);
            _order = GetValueOrDefault(_order, "OrderId");
            base.DefaultOnNull();
        }
    }
}
