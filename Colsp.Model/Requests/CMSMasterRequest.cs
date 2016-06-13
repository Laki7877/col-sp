using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSMasterRequest : PaginatedRequest
    {
        public CMSMasterRequest()
        {
            this.ScheduleList = new List<CMSSchedulerRequest>();
        }

        public int CMSMasterId { get; set; }
        public string CMSMasterNameEN { get; set; }
        public string CMSMasterNameTH { get; set; }
        public string CMSMasterURLKey { get; set; }
        public string CMSMasterType { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public string MobileShortDescriptionTH { get; set; }
        public string MobileLongDescriptionTH { get; set; }
        public string MobileShortDescriptionEN { get; set; }
        public string MobileLongDescriptionEN { get; set; }
        public string Status { get; set; }
        public bool Visibility { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateOn { get; set; }
        public string CreateIP { get; set; }
        public bool ISCampaign { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }

        public List<CMSSchedulerRequest> ScheduleList { get; set; }
        public List<CMSFeatureProductRequest> FeatureProductList { get; set; }
        public List<CMSCategoryRequest> CategoryList{ get; set; }
        public List<ImageRequest> CMSBannerEN { get; set; }
        public List<ImageRequest> CMSBannerTH { get; set; }


        public string SearchText { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
        public string UpdateIP { get; set; }
        

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "CMSMasterId");
            base.DefaultOnNull();
        }
    }

    public class CMSMasterCategoryMapRequest
    {
        public int CMSMasterCategoryMapId { get; set; }
        public int CMSMasterId { get; set; }
        public int CMSCategoryId { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
    }

    public class CMSFeatureProductRequest
    {
        public int CMSMasterId { get; set; }
        public string ProductId { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
    }
}
