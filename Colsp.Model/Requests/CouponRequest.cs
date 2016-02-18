using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CouponRequest : PaginatedRequest
    {
        public int? CouponId { get; set; }
        public string CouponCode { get; set; }
        public string CouponName { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public string Status { get; set; }
        public int? Remaining { get; set; }
        public string SearchText { get; set; }


        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "CouponName");
            base.DefaultOnNull();
        }

    }
}
