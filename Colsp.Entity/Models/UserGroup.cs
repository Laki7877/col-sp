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
    
    public partial class UserGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserGroup()
        {
            this.ShopUserGroupMaps = new HashSet<ShopUserGroupMap>();
            this.UserGroupMaps = new HashSet<UserGroupMap>();
            this.UserGroupPermissionMaps = new HashSet<UserGroupPermissionMap>();
        }
    
        public int GroupId { get; set; }
        public string GroupNameEn { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateOn { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserGroupMap> UserGroupMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserGroupPermissionMap> UserGroupPermissionMaps { get; set; }
    }
}
