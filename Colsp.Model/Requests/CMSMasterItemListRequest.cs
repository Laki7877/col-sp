using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSMasterItemListRequest : PaginatedRequest
    {
        public string SearchText { get; set; }
        public string CreateIP { get; set; }
        public List<CMSItemUpdate> CMSMasterList { get; set; }
    }

    public class CMSItemUpdate
    {
        public int CMSMasterId { get; set; }
        public int CMSMasterStatusId { get; set; }
        public bool CMSMasterVisibility { get; set; }
    }

}
