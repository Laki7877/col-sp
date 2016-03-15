using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class UserGroupRequest : PaginatedRequest
    {
        public int GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public string GroupNameTh { get; set; }
        public List<PermissionRequest> Permission { get; set; }
        public string SearchText { get; set; }

        public UserGroupRequest()
        {
            GroupId = 0;
            GroupNameEn = string.Empty;
            GroupNameTh = string.Empty;
            SearchText = string.Empty;
            Permission = new List<PermissionRequest>();
        }


        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            GroupNameEn = GetValueOrDefault(GroupNameEn, string.Empty);
            GroupNameTh = GetValueOrDefault(GroupNameEn, string.Empty);
            _order = GetValueOrDefault(_order, "GroupId");
            base.DefaultOnNull();
        }

        
    }
}
