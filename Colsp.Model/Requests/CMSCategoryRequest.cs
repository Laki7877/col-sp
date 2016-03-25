using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSCategoryRequest
    {

    }
    public class MasterGetCategoryRequest:PaginatedRequest {
        public bool? IsCampaign { get; set; }
        public string SearchText { get; set; }
    }

}
