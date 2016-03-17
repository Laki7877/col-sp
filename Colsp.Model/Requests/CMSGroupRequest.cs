using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSGroupRequest
    {
        public Nullable<int> CMSGroupId { get; set; }
        public string CMSGroupNameEN { get; set; }
        public string CMSGroupNameTH { get; set; }
        public int Sequence { get; set; }
        public bool Status { get; set; }
        public bool Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string CreateIP { get; set; }
    }
}
