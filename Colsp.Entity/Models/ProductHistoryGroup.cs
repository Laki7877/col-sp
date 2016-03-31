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
    
    public partial class ProductHistoryGroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductHistoryGroup()
        {
            this.ProductHistories = new HashSet<ProductHistory>();
            this.ProductHistoryGlobalCatMaps = new HashSet<ProductHistoryGlobalCatMap>();
            this.ProductHistoryLocalCatMaps = new HashSet<ProductHistoryLocalCatMap>();
            this.ProductHistoryTags = new HashSet<ProductHistoryTag>();
        }
    
        public long HistoryId { get; set; }
        public long ProductId { get; set; }
        public Nullable<int> ShopId { get; set; }
        public Nullable<int> GlobalCatId { get; set; }
        public Nullable<int> LocalCatId { get; set; }
        public Nullable<int> AttributeSetId { get; set; }
        public Nullable<int> BrandId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.DateTime> ExpireDate { get; set; }
        public bool ControlFlag1 { get; set; }
        public bool ControlFlag2 { get; set; }
        public bool ControlFlag3 { get; set; }
        public string Remark { get; set; }
        public bool InfoFlag { get; set; }
        public bool ImageFlag { get; set; }
        public bool OnlineFlag { get; set; }
        public string InformationTabStatus { get; set; }
        public string ImageTabStatus { get; set; }
        public string CategoryTabStatus { get; set; }
        public string VariantTabStatus { get; set; }
        public string MoreOptionTabStatus { get; set; }
        public string RejectReason { get; set; }
        public bool Visibility { get; set; }
        public string Status { get; set; }
        public System.DateTime HistoryDt { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDt { get; set; }
        public string SubmittedBy { get; set; }
        public Nullable<System.DateTime> SubmittedDt { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
    
        public virtual AttributeSet AttributeSet { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual GlobalCategory GlobalCategory { get; set; }
        public virtual LocalCategory LocalCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistory> ProductHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryGlobalCatMap> ProductHistoryGlobalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryLocalCatMap> ProductHistoryLocalCatMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductHistoryTag> ProductHistoryTags { get; set; }
    }
}
