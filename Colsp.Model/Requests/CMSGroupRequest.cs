using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSGroupRequest : PaginatedRequest
    {
        public CMSGroupRequest()
        {
            this.GroupMasterList = new List<CMSMasterGroupMapRequest>();
        }

        public int CMSGroupId { get; set; }
        public string CMSGroupNameEN { get; set; }
        public string CMSGroupNameTH { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public bool Visibility { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public List<CMSMasterGroupMapRequest> GroupMasterList { get; set; }

        public string SearchText { get; set; }
        public int ShopId { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "ShopId");
            base.DefaultOnNull();
        }
    }

    public class CMSMasterGroupMapRequest
    {
        public int CMSMasterGroupMapId { get; set; }
        public int? CMSGroupId { get; set; }
        public int? CMSMasterId { get; set; }
        public int? Sequence { get; set; }
        public int? ShopId { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public DateTime? CMSMasterExpiryDate { get; set; }

        public string CMSMasterNameEN { get; set; }
        public string CMSMasterNameTH { get; set; }
    }
    public class CMSGroupListRequest : PaginatedRequest
    {
        public string SearchText { get; set; }
    }
}
