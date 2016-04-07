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

    public partial class Inventory
    {
        public string Pid { get; set; }
        public int Quantity { get; set; }
        public int Defect { get; set; }
        public int OnHold { get; set; }
        public int Reserve { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public bool UseDecimal { get; set; }
        public int SafetyStockSeller { get; set; }
        public int SafetyStockAdmin { get; set; }
        public int StockAvailable { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual ProductStage ProductStage { get; set; }
    }
}
