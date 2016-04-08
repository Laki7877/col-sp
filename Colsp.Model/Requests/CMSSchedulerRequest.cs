using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSSchedulerRequest
    {
        public CMSSchedulerRequest()
        {
            this.CategoryList = new List<CMSCategoryRequest>();
        }

        public int CMSMasterId { get; set; }
        public int CMSSchedulerId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public List<CMSCategoryRequest> CategoryList { get; set; }
        public bool? Status { get; set; }
        public bool? Visibility { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateIP { get; set; }
    }
}
