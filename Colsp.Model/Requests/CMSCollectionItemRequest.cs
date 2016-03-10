using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSCollectionItemRequest
    {
        public int CMSId { get; set; }
        public string CMSNameEN { get; set; }
        public string CMSNameTH { get; set; }
        public string URLKey { get; set; }
        public Nullable<int> CMSTypeId { get; set; }
        public Nullable<int> CMSSortId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<int> ShopId { get; set; }
        public Nullable<int> CMSCount { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string CreateIP { get; set; }
        public List<CollectionItemList> CollectionItemList { get; set; }
        public int? CMSStatusFlowId { get; set; }
        public int? Sequence { get; set; }
        public int? CMSCollectionGroupId { get; set; }
    }
    public class CollectionItemList {
        public int? CMSCollectionItemGroupId { get; set; }
        public int CMSCollectionItemId { get; set; }
        public string PId { get; set; }
        public string ProductBoxBadge { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<bool> Status { get; set; }
    }    
}