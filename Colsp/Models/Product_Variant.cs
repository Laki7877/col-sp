//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Api.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product_Variant
    {
        public int product_id { get; set; }
        public int variant_id { get; set; }
        public string value_en { get; set; }
        public string value_th { get; set; }
        public string product_name_en { get; set; }
        public string product_name_th { get; set; }
        public string uid { get; set; }
        public string pid { get; set; }
        public string sku { get; set; }
        public string upc { get; set; }
        public Nullable<decimal> original_price { get; set; }
        public Nullable<decimal> sale_price { get; set; }
        public string description_full_en { get; set; }
        public string description_short_en { get; set; }
        public string description_full_th { get; set; }
        public string description_short_th { get; set; }
        public Nullable<int> stock { get; set; }
        public Nullable<int> safty_stock { get; set; }
        public string video_link { get; set; }
        public string shipping_method { get; set; }
        public Nullable<decimal> length { get; set; }
        public Nullable<decimal> height { get; set; }
        public Nullable<decimal> width { get; set; }
        public string dimenstion_unit { get; set; }
        public Nullable<decimal> weight { get; set; }
        public Nullable<decimal> dimenstion_weight { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_dt { get; set; }
        public string updated_by { get; set; }
        public Nullable<System.DateTime> updated_dt { get; set; }
    
        public virtual Attribute Attribute { get; set; }
        public virtual Product Product { get; set; }
    }
}
