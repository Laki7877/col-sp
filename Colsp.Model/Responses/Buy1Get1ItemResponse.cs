﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class Buy1Get1ItemResponse
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
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
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
        public string PNameBuy { get; set; }
        public string PNameGet { get; set; }
    }
}
