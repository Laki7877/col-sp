using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSMasterMappingItemCategoryRequest : PaginatedRequest
    {
        public int CMSMasterId { get; set; }
        public string CreateIP { get; set; }

        public List<CategoryItemMapRequest> CategoryList { get; set; }
    }

    public class CategoryItemMapRequest {
        public int CMSCategoryId { get; set; }
    }
}

