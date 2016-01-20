﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ColspEntities : DbContext
    {
        public ColspEntities()
            : base("name=ColspEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Attribute> Attributes { get; set; }
        public virtual DbSet<AttributeSet> AttributeSets { get; set; }
        public virtual DbSet<AttributeSetMap> AttributeSetMaps { get; set; }
        public virtual DbSet<AttributeSetTagMap> AttributeSetTagMaps { get; set; }
        public virtual DbSet<AttributeValue> AttributeValues { get; set; }
        public virtual DbSet<AttributeValueMap> AttributeValueMaps { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<CategoryAttributeSetMap> CategoryAttributeSetMaps { get; set; }
        public virtual DbSet<GlobalCategory> GlobalCategories { get; set; }
        public virtual DbSet<GlobalCategoryAbbrevation> GlobalCategoryAbbrevations { get; set; }
        public virtual DbSet<GlobalCategoryPID> GlobalCategoryPIDs { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<InventoryHistory> InventoryHistories { get; set; }
        public virtual DbSet<LocalCategory> LocalCategories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }
        public virtual DbSet<ProductGlobalCatMap> ProductGlobalCatMaps { get; set; }
        public virtual DbSet<ProductHistory> ProductHistories { get; set; }
        public virtual DbSet<ProductHistoryAttribute> ProductHistoryAttributes { get; set; }
        public virtual DbSet<ProductHistoryGlobalCatMap> ProductHistoryGlobalCatMaps { get; set; }
        public virtual DbSet<ProductHistoryImage> ProductHistoryImages { get; set; }
        public virtual DbSet<ProductHistoryLocalCatMap> ProductHistoryLocalCatMaps { get; set; }
        public virtual DbSet<ProductHistoryRelated> ProductHistoryRelateds { get; set; }
        public virtual DbSet<ProductHistoryVariant> ProductHistoryVariants { get; set; }
        public virtual DbSet<ProductHistoryVideo> ProductHistoryVideos { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<ProductLocalCatMap> ProductLocalCatMaps { get; set; }
        public virtual DbSet<ProductRelated> ProductRelateds { get; set; }
        public virtual DbSet<ProductStage> ProductStages { get; set; }
        public virtual DbSet<ProductStageAttribute> ProductStageAttributes { get; set; }
        public virtual DbSet<ProductStageGlobalCatMap> ProductStageGlobalCatMaps { get; set; }
        public virtual DbSet<ProductStageImage> ProductStageImages { get; set; }
        public virtual DbSet<ProductStageImage360> ProductStageImage360 { get; set; }
        public virtual DbSet<ProductStageLocalCatMap> ProductStageLocalCatMaps { get; set; }
        public virtual DbSet<ProductStageRelated> ProductStageRelateds { get; set; }
        public virtual DbSet<ProductStageVariant> ProductStageVariants { get; set; }
        public virtual DbSet<ProductStageVideo> ProductStageVideos { get; set; }
        public virtual DbSet<ProductVariant> ProductVariants { get; set; }
        public virtual DbSet<ProductVideo> ProductVideos { get; set; }
        public virtual DbSet<Shipping> Shippings { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<UserGroupMap> UserGroupMaps { get; set; }
        public virtual DbSet<UserGroupPermissionMap> UserGroupPermissionMaps { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<UserShop> UserShops { get; set; }
    }
}
