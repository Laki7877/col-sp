//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Api.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inventory
    {
        public string Uid { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> QuantityOutStock { get; set; }
        public Nullable<int> MinQuantity { get; set; }
        public Nullable<int> MaxQuantity { get; set; }
        public Nullable<bool> UseDecimal { get; set; }
        public Nullable<int> SaftyStockSeller { get; set; }
        public Nullable<int> SaftyStockAdmin { get; set; }
        public Nullable<int> StockAvailable { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    }
}
