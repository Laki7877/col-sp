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
    
    public partial class StockStatusReport_Result
    {
        public string Pid { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameTh { get; set; }
        public string variant1 { get; set; }
        public string variant2 { get; set; }
        public int OnHand { get; set; }
        public int Reserve { get; set; }
        public int OnHold { get; set; }
        public int Defect { get; set; }
        public Nullable<int> StockAvailable { get; set; }
        public int FirstReceiveQTY { get; set; }
        public string FirstReceiveDate { get; set; }
        public int SafetyStockAdmin { get; set; }
        public int SafetyStockSeller { get; set; }
        public string LastSoldDate { get; set; }
        public decimal SalePrice { get; set; }
        public Nullable<int> AgingDay { get; set; }
    }
}