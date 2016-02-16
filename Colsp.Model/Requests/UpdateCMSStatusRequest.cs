using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class UpdateCMSStatusRequest : PaginatedRequest
    {
        public int? ShopId { get; set; }
        public string SearchText { get; set; }
        public int UserId { get; set; }
        public string CreateIP { get; set; }
        public List<CMSAsUpdate> CMSList { get; set; }
    }

    public class CMSAsUpdate
    {
        public int CMSId { get; set; }
        public int CMSStatusId { get; set; }
        public bool CMSVisibility { get; set; }
        public bool Status { get; set; }
    }

}
