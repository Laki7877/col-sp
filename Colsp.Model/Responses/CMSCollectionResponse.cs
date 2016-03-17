using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CMSCollectionResponse
    {
        public int CMSMasterId { get; set; }
        public string CMSMasterNameEN { get; set; }
        public string CMSMasterNameTH { get; set; }
        public string CMSMasterURLKey { get; set; }
        public Nullable<System.DateTime> CMSMasterEffectiveDate { get; set; }
        public Nullable<System.TimeSpan> CMSMasterEffectiveTime { get; set; }
        public Nullable<System.DateTime> CMSMasterExpiryDate { get; set; }
        public Nullable<System.TimeSpan> CMSMasterExpiryTime { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public string CreateBy { get; set; }
        public string CreateIP { get; set; }
        public DateTime CreateDate { get; set; }
        public string  UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<CategoryListResponse> CategoryLists { get; set; }
        public string CMSType { get; set; }
        public int? CMSMasterStatusId { get; set; }
        public string CMSStatus { get; set; }
    }
    public class CategoryListResponse
    {
        public int CMSMasterId { get; set; }
        public int CMSCategoryId { get; set; }
        public string CategoryNameEN { get; set; }
        public string CategoryNameTH { get; set; }
        public Nullable<int> Sequence { get; set; }
        public List<ProductListResponse> ProductLists { get; set; }
    }

    public class ProductListResponse
    {
        public int CMSMasterId { get; set; }
        public int CMSCategoryId { get; set; }
        public string ProductPId { get; set; }
        public string ProductNameEN { get; set; }
        public string ProductNameTH { get; set; }
        public string SKU { get; set; }
        public string ProductBoxBadge { get; set; }
        public Nullable<int> Sequence { get; set; }
        public bool? Status { get; set; } //Approve?
        public bool? IsActive { get; set; }
    }

}
