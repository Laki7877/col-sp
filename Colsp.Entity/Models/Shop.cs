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
    
    public partial class Shop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Shop()
        {
            this.LocalCategories = new HashSet<LocalCategory>();
            this.Products = new HashSet<Product>();
            this.ProductHistories = new HashSet<ProductHistory>();
            this.ProductStages = new HashSet<ProductStage>();
            this.ProductStageImages = new HashSet<ProductStageImage>();
            this.ProductStageImage360 = new HashSet<ProductStageImage360>();
            this.ProductStageRelateds = new HashSet<ProductStageRelated>();
            this.ProductStageVariants = new HashSet<ProductStageVariant>();
            this.ProductStageVideos = new HashSet<ProductStageVideo>();
            this.ShopUserGroupMaps = new HashSet<ShopUserGroupMap>();
            this.UserShops = new HashSet<UserShop>();
        }
    
        public int ShopId { get; set; }
        public Nullable<int> ShopOwner { get; set; }
        public string ShopNameEn { get; set; }
        public string ShopNameTh { get; set; }
        public Nullable<decimal> Commission { get; set; }
        public Nullable<int> ShopTypeId { get; set; }
        public string UrlKeyEn { get; set; }
        public string UrlKeyTh { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalCategory> LocalCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistory> ProductHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStage> ProductStages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageImage> ProductStageImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageImage360> ProductStageImage360 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageRelated> ProductStageRelateds { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVariant> ProductStageVariants { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductStageVideo> ProductStageVideos { get; set; }
        public virtual ShopType ShopType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserShop> UserShops { get; set; }
    }
}
