using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class UserGroupRequest : PaginatedRequest
    {
        public int? GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public string GroupNameTh { get; set; }
        public List<PermissionRequest> Permission { get; set; }
        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, "");
            GroupNameEn = GetValueOrDefault(GroupNameEn, "");
            GroupNameTh = GetValueOrDefault(GroupNameEn, "");
            _order = GetValueOrDefault(_order, "GroupId");
            base.DefaultOnNull();
        }

        public UserGroupRequest()
        {
            Permission = new List<PermissionRequest>();
        }
    }
}
