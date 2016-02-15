using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSShopRequest : PaginatedRequest
    {
        //public int? ShopId { get; set; }
        public string SearchText { get; set; }
    }
}
