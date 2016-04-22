using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class Buy1Get1ItemRequest
    {
        public int PromotionBuy1Get1ItemId { get; set; }
        public string NameEN { get; set; }
        public string NameTH { get; set; }
        public string URLKey { get; set; }
        public Nullable<int> PIDBuy { get; set; }
        public Nullable<int> PIDGet { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public string ProductBoxBadge { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public Nullable<int> CMSStatusFlowId { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public string CampaignID { get; set; }
        public string CampaignName { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionCodeRef { get; set; }
        public int? MarketingAbsorb { get; set; }
        public int? MerchandiseAbsorb { get; set; }
        public int? VendorAbsorb { get; set; }
        public List<ProductBuy> ProductBuyList { get; set; }
        public List<ProductGet> ProductGetList { get; set; }
    }

    public class ProductBuy {
        public int Pid { get; set; }
    }


    public class ProductGet
    {
        public int Pid { get; set; }
    }
}
