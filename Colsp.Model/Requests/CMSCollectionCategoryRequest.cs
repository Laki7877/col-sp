using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSCollectionCategoryRequest
    {
        public string CMSCollectionCategoryNameEN { get; set; }
        public string CMSCollectionCategoryNameTH { get; set; }
        public int? CMSStatusFlowId { get; set; }
        public int? CreateBy { get; set; }
        public string CreateIP { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public TimeSpan? EffectiveTime { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public TimeSpan? ExpiryTime { get; set; }
        public int? Sequence { get; set; }
        public bool? Status { get; set; }
        public int? UpdateBy { get; set; }
        public string UpdateIP { get; set; }
        public bool? Visibility { get; set; }
    }
}
