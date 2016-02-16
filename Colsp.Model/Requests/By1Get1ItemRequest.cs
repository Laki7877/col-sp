using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class By1Get1ItemRequest
    {
        public int CMSId { get; set; }
        public string NameEN { get; set; }
        public string NameTH { get; set; }
        public string URLKey { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<int> ShopId { get; set; }
        public string ShortDetailTH { get; set; }
        public string ShortDetailEN { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string ShortDescriptionTH { get; set; }
        public int? Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string CreateIP { get; set; }
        public int? CMSStatusFlowId { get; set; }
        public int ByPID { get; set; }
        public int GetPID { get; set; }
    }
}
