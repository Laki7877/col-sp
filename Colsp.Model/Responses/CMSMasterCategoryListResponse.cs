using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colsp.Model.Requests;

namespace Colsp.Model.Responses
{
    public class CMSMasterCategoryListResponse : PaginatedResponse
    {
        public int CMSMasterId { get; set; }
        public List<CMSCategoryRequest> CategoryList { get; set; }
    }
}
