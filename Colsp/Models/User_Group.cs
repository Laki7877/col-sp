//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Colsp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User_Group()
        {
            this.User_Group_Map = new HashSet<User_Group_Map>();
            this.Roles = new HashSet<Role>();
        }
    
        public int group_id { get; set; }
        public string group_name_en { get; set; }
        public string group_name_th { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_dt { get; set; }
        public string updated_by { get; set; }
        public Nullable<System.DateTime> updated_dt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User_Group_Map> User_Group_Map { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Role> Roles { get; set; }
    }
}
