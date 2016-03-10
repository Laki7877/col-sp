using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CMSCollectionResponse
    {
        public int CMSId { get; set; }
        public string CMSNameEN { get; set; }
        public string CMSNameTH { get; set; }
        public string URLKey { get; set; }
        public Nullable<int> CMSTypeId { get; set; }
        public Nullable<int> CMSFilterId { get; set; }
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
        public int? CMSStatusFlowId { get; set; }
        public int? CMSCollectionGroupId { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public string CreateIP { get; set; }
        public DateTime CreateDate { get; set; }
        public List<CollectionItemListResponse> CollectionItemList { get; set; }
       
    }
    public class CollectionItemListResponse
    {
        public int CMSId { get; set; }
        public string PId { get; set; }
        public string ProductBoxBadge { get; set; }
        public Nullable<int> Sequence { get; set; }
        public bool? Status { get; set; }
        public int? CMSCollectionItemGroupId { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public partial class CMSRelProductCategoryResponse
    {
        public int CMSRelProductCategoryId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<int> CMSCollectionCategoryId { get; set; }
    }
    public class CMSCollectionCategoryResponse
    {
        public int CMSCollectionCategoryId { get; set; }
        public string CMSCollectionCategoryNameEH { get; set; }
        public string CMSCollectionCategoryNameTH { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<int> CMSStatusFlowId { get; set; }
        public Nullable<int> Sequence { get; set; }
        public List<CMSRelProductCategoryResponse> CMSRelProductCategory { get; set; }
    }
}
