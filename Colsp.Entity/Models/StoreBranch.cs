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
    
    public partial class StoreBranch
    {
        public string BranchCode { get; set; }
        public string BranchThaiName { get; set; }
        public string BranchEnglishName { get; set; }
        public string BranchABB { get; set; }
        public string AddressTH { get; set; }
        public string AddressEN { get; set; }
        public string SubDistrictTH { get; set; }
        public string SubDistrictEN { get; set; }
        public string DistrictTH { get; set; }
        public string DistrictEN { get; set; }
        public int ProvinceId { get; set; }
        public string Postcode { get; set; }
        public int CollectionFloor { get; set; }
        public int Status { get; set; }
        public string MapPath { get; set; }
        public string BranchType { get; set; }
    }
}
