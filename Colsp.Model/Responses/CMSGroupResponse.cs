using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CMSGroupResponse
    {
        public int CMSGroupId { get; set; }
        public string CMSGroupNameEN { get; set; }
        public string CMSGroupNameTH { get; set; }
        public string BannerLocation { get; set; }
        public string BannerConntent { get; set; }
        public int Sequence { get; set; }
        public bool Status { get; set; }
        public bool Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string CreateIP { get; set; }
        public int? ShopId { get; set; }
    }
}
