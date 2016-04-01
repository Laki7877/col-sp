using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CMSMasterResponse
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
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public List<SchedulerListResponse> SchedulerLists { get; set; }
        public string CMSType { get; set; }
        public int? CMSMasterStatusId { get; set; }
        public string CMSStatus { get; set; }

        public CMSMasterResponse()
        {
            CMSMasterId = 0;
            CMSMasterNameEN = string.Empty;
            CMSMasterNameTH = string.Empty;
            CMSMasterURLKey = string.Empty;
            CMSMasterEffectiveDate = null;
            CMSMasterEffectiveTime = null;
            CMSMasterExpiryDate = null;
            CMSMasterExpiryTime = null;
            ShortDescriptionTH = string.Empty;
            LongDescriptionTH = string.Empty;
            ShortDescriptionEN = string.Empty;
            LongDescriptionEN = string.Empty;
            Status = null;
            Visibility = null;
            CreateBy = string.Empty;
            CreateIP = string.Empty;
            CreateDate = null;
            UpdateBy = string.Empty;
            UpdateDate = null;
            SchedulerLists = new List<SchedulerListResponse>();
            CMSType = string.Empty;
            CMSMasterStatusId = 0;
            CMSStatus = string.Empty;
        }
    }
    public class SchedulerListResponse
    {
        public int CMSSchedulerId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public List<CategoryListResponse> CategoryLists { get; set; }
        public SchedulerListResponse() {
            CategoryLists = new List<CategoryListResponse>();
        }
    }

    public class CategoryListResponse
    {
        public int CMSMasterId { get; set; }
        public int CMSCategoryId { get; set; }
        public string CategoryNameEN { get; set; }
        public string CategoryNameTH { get; set; }
        public Nullable<int> Sequence { get; set; }
        public List<CriteriaListResponse> CriteriaLists { get; set; }
        public CategoryListResponse() {
            CriteriaLists = new List<CriteriaListResponse>();
        }

    }

    public class CriteriaListResponse {
        public Nullable<int> CategoryId { get; set; }
        public Nullable<int> Brand { get; set; }
        public Nullable<decimal> MinPrice { get; set; }
        public Nullable<decimal> MaxPrice { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public List<ProductListResponse> ProductLists { get; set; }
        public CriteriaListResponse()
        {
            ProductLists = new List<ProductListResponse>();
        }
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
