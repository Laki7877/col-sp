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
    
    public partial class AppAuth
    {
        public string AppId { get; set; }
        public string SecretKey { get; set; }
        public string AppName { get; set; }
        public Nullable<System.DateTime> ExpiredDate { get; set; }
        public System.DateTime CreateOn { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
        public string UpdateBy { get; set; }
    }
}
