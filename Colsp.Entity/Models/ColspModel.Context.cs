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
            this.Configuration.LazyLoadingEnabled = false;
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
        public virtual DbSet<BrandFeatureProduct> BrandFeatureProducts { get; set; }
        public virtual DbSet<BrandImage> BrandImages { get; set; }
        public virtual DbSet<CategoryAttributeSetMap> CategoryAttributeSetMaps { get; set; }
        public virtual DbSet<CMSBrandInShop> CMSBrandInShops { get; set; }
        public virtual DbSet<CMSCollectionCategory> CMSCollectionCategories { get; set; }
        public virtual DbSet<CMSCollectionGroup> CMSCollectionGroups { get; set; }
        public virtual DbSet<CMSCollectionListItem> CMSCollectionListItems { get; set; }
        public virtual DbSet<CMSFilter> CMSFilters { get; set; }
        public virtual DbSet<CMSHistoryLog> CMSHistoryLogs { get; set; }
        public virtual DbSet<CMSMainCategory> CMSMainCategories { get; set; }
        public virtual DbSet<CMSMaster> CMSMasters { get; set; }
        public virtual DbSet<CMSRelCollectionCategory> CMSRelCollectionCategories { get; set; }
        public virtual DbSet<CMSRelCollectionGroup> CMSRelCollectionGroups { get; set; }
        public virtual DbSet<CMSStatusFlow> CMSStatusFlows { get; set; }
        public virtual DbSet<CMSType> CMSTypes { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<CouponBrandMap> CouponBrandMaps { get; set; }
        public virtual DbSet<CouponCondition> CouponConditions { get; set; }
        public virtual DbSet<CouponCustomerMap> CouponCustomerMaps { get; set; }
        public virtual DbSet<CouponGlobalCatMap> CouponGlobalCatMaps { get; set; }
        public virtual DbSet<CouponLocalCatMap> CouponLocalCatMaps { get; set; }
        public virtual DbSet<CouponOrder> CouponOrders { get; set; }
        public virtual DbSet<CouponPidMap> CouponPidMaps { get; set; }
        public virtual DbSet<CouponShopMap> CouponShopMaps { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<GlobalCategory> GlobalCategories { get; set; }
        public virtual DbSet<GlobalCategoryAbbrevation> GlobalCategoryAbbrevations { get; set; }
        public virtual DbSet<GlobalCategoryPID> GlobalCategoryPIDs { get; set; }
        public virtual DbSet<GlobalCatFeatureProduct> GlobalCatFeatureProducts { get; set; }
        public virtual DbSet<GlobalCatImage> GlobalCatImages { get; set; }
        public virtual DbSet<ImportHeader> ImportHeaders { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<InventoryHistory> InventoryHistories { get; set; }
        public virtual DbSet<LocalCategory> LocalCategories { get; set; }
        public virtual DbSet<LocalCatFeatureProduct> LocalCatFeatureProducts { get; set; }
        public virtual DbSet<LocalCatImage> LocalCatImages { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
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
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<ProductStage> ProductStages { get; set; }
        public virtual DbSet<ProductStageAttribute> ProductStageAttributes { get; set; }
        public virtual DbSet<ProductStageComment> ProductStageComments { get; set; }
        public virtual DbSet<ProductStageGlobalCatMap> ProductStageGlobalCatMaps { get; set; }
        public virtual DbSet<ProductStageImage> ProductStageImages { get; set; }
        public virtual DbSet<ProductStageImage360> ProductStageImage360 { get; set; }
        public virtual DbSet<ProductStageLocalCatMap> ProductStageLocalCatMaps { get; set; }
        public virtual DbSet<ProductStageRelated> ProductStageRelateds { get; set; }
        public virtual DbSet<ProductStageVariant> ProductStageVariants { get; set; }
        public virtual DbSet<ProductStageVariantArrtibuteMap> ProductStageVariantArrtibuteMaps { get; set; }
        public virtual DbSet<ProductStageVideo> ProductStageVideos { get; set; }
        public virtual DbSet<ProductTag> ProductTags { get; set; }
        public virtual DbSet<ProductVideo> ProductVideos { get; set; }
        public virtual DbSet<PromotionBuy1Get1Item> PromotionBuy1Get1Item { get; set; }
        public virtual DbSet<PromotionOnTopCreditCard> PromotionOnTopCreditCards { get; set; }
        public virtual DbSet<PromotionOnTopCreditNumber> PromotionOnTopCreditNumbers { get; set; }
        public virtual DbSet<Shipping> Shippings { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<ShopCommission> ShopCommissions { get; set; }
        public virtual DbSet<ShopImage> ShopImages { get; set; }
        public virtual DbSet<ShopType> ShopTypes { get; set; }
        public virtual DbSet<ShopTypePermissionMap> ShopTypePermissionMaps { get; set; }
        public virtual DbSet<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<UserGroupMap> UserGroupMaps { get; set; }
        public virtual DbSet<UserGroupPermissionMap> UserGroupPermissionMaps { get; set; }
        public virtual DbSet<UserShop> UserShops { get; set; }
        public virtual DbSet<UserStatu> UserStatus { get; set; }
    }
}
