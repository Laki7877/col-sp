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
    
    public partial class CustomerTokenCreditCard
    {
        public int CardId { get; set; }
        public string CustomerId { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string Token { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    }
}
