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
    
    public partial class PostCodeMap
    {
        public int ProvinceId { get; set; }
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int PostCodeId { get; set; }
        public string PostCode { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceNameEn { get; set; }
        public string CityName { get; set; }
        public string CityNameEn { get; set; }
        public string DistrictName { get; set; }
        public string DistrictNameEn { get; set; }
        public string Remark { get; set; }
    }
}