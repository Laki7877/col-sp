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
    
    public partial class Payment
    {
        public int PaymentID { get; set; }
        public string PaymentCode { get; set; }
        public string PaymentType { get; set; }
        public string PaymentName { get; set; }
        public string PaymentNameEN { get; set; }
        public decimal PaymentDiscount { get; set; }
        public int PaymentStatus { get; set; }
        public string Createby { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> Updateon { get; set; }
        public int SeqNo { get; set; }
    }
}
