//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Entity.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CMSMaster
    {
        public int CMSMasterId { get; set; }
        public string CMSMasterNameEN { get; set; }
        public string CMSMasterNameTH { get; set; }
        public string CMSMasterURLKey { get; set; }
        public Nullable<int> CMSTypeId { get; set; }
        public Nullable<System.DateTime> CMSMasterEffectiveDate { get; set; }
        public Nullable<System.TimeSpan> CMSMasterEffectiveTime { get; set; }
        public Nullable<System.DateTime> CMSMasterExpiryDate { get; set; }
        public Nullable<System.TimeSpan> CMSMasterExpiryTime { get; set; }
        public Nullable<int> CMSMasterTotal { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<int> CMSMasterStatusId { get; set; }
        public string LinkToOutside { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
    }
}
