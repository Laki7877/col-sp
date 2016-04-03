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
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class ColspEntities : DbContext
    {
        public ColspEntities()
            : base("name=ColspEntities")
        {
            Configuration.LazyLoadingEnabled = false;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AppAuth> AppAuths { get; set; }
        public virtual DbSet<Attribute> Attributes { get; set; }
        public virtual DbSet<AttributeSet> AttributeSets { get; set; }
        public virtual DbSet<AttributeSetMap> AttributeSetMaps { get; set; }
        public virtual DbSet<AttributeSetTag> AttributeSetTags { get; set; }
        public virtual DbSet<AttributeValue> AttributeValues { get; set; }
        public virtual DbSet<AttributeValueMap> AttributeValueMaps { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<BrandFeatureProduct> BrandFeatureProducts { get; set; }
        public virtual DbSet<BrandImage> BrandImages { get; set; }
        public virtual DbSet<CartDetail> CartDetails { get; set; }
        public virtual DbSet<CartDiscount> CartDiscounts { get; set; }
        public virtual DbSet<CartHead> CartHeads { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<CMSCategory> CMSCategories { get; set; }
        public virtual DbSet<CMSCategoryCreteriaMap> CMSCategoryCreteriaMaps { get; set; }
        public virtual DbSet<CMSCategoryProductMap> CMSCategoryProductMaps { get; set; }
        public virtual DbSet<CMSCategorySchedulerMap> CMSCategorySchedulerMaps { get; set; }
        public virtual DbSet<CMSCreteria> CMSCreterias { get; set; }
        public virtual DbSet<CMSCriteriaProductMap> CMSCriteriaProductMaps { get; set; }
        public virtual DbSet<CMSGroup> CMSGroups { get; set; }
        public virtual DbSet<CMSMaster> CMSMasters { get; set; }
        public virtual DbSet<CMSMasterGroupMap> CMSMasterGroupMaps { get; set; }
        public virtual DbSet<CMSMasterSchedulerMap> CMSMasterSchedulerMaps { get; set; }
        public virtual DbSet<CMSMasterSchedulerShopMap> CMSMasterSchedulerShopMaps { get; set; }
        public virtual DbSet<CMSMasterStatu> CMSMasterStatus { get; set; }
        public virtual DbSet<CMSMasterType> CMSMasterTypes { get; set; }
        public virtual DbSet<CMSScheduler> CMSSchedulers { get; set; }
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
        public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public virtual DbSet<CustomerToken> CustomerTokens { get; set; }
        public virtual DbSet<CustomerWishList> CustomerWishLists { get; set; }
        public virtual DbSet<Deal> Deals { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<GlobalCatAttributeSetMap> GlobalCatAttributeSetMaps { get; set; }
        public virtual DbSet<GlobalCategory> GlobalCategories { get; set; }
        public virtual DbSet<GlobalCatFeatureProduct> GlobalCatFeatureProducts { get; set; }
        public virtual DbSet<GlobalCatImage> GlobalCatImages { get; set; }
        public virtual DbSet<ImportHeader> ImportHeaders { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<InventoryHistory> InventoryHistories { get; set; }
        public virtual DbSet<LocalCategory> LocalCategories { get; set; }
        public virtual DbSet<LocalCatFeatureProduct> LocalCatFeatureProducts { get; set; }
        public virtual DbSet<LocalCatImage> LocalCatImages { get; set; }
        public virtual DbSet<Newsletter> Newsletters { get; set; }
        public virtual DbSet<NewsletterShopMap> NewsletterShopMaps { get; set; }
        public virtual DbSet<ODMPermission> ODMPermissions { get; set; }
        public virtual DbSet<ODMProcessLog> ODMProcessLogs { get; set; }
        public virtual DbSet<ODMRole> ODMRoles { get; set; }
        public virtual DbSet<ODMRolePermission> ODMRolePermissions { get; set; }
        public virtual DbSet<ODMUser> ODMUsers { get; set; }
        public virtual DbSet<ODMUserMenuItem> ODMUserMenuItems { get; set; }
        public virtual DbSet<ODMUserToken> ODMUserTokens { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Pid> Pids { get; set; }
        public virtual DbSet<PostCode> PostCodes { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }
        public virtual DbSet<ProductGlobalCatMap> ProductGlobalCatMaps { get; set; }
        public virtual DbSet<ProductHistory> ProductHistories { get; set; }
        public virtual DbSet<ProductHistoryAttribute> ProductHistoryAttributes { get; set; }
        public virtual DbSet<ProductHistoryGlobalCatMap> ProductHistoryGlobalCatMaps { get; set; }
        public virtual DbSet<ProductHistoryGroup> ProductHistoryGroups { get; set; }
        public virtual DbSet<ProductHistoryImage> ProductHistoryImages { get; set; }
        public virtual DbSet<ProductHistoryLocalCatMap> ProductHistoryLocalCatMaps { get; set; }
        public virtual DbSet<ProductHistoryTag> ProductHistoryTags { get; set; }
        public virtual DbSet<ProductHistoryVideo> ProductHistoryVideos { get; set; }
        public virtual DbSet<ProductLocalCatMap> ProductLocalCatMaps { get; set; }
        public virtual DbSet<ProductRelated> ProductRelateds { get; set; }
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<ProductStage> ProductStages { get; set; }
        public virtual DbSet<ProductStageAttribute> ProductStageAttributes { get; set; }
        public virtual DbSet<ProductStageComment> ProductStageComments { get; set; }
        public virtual DbSet<ProductStageGlobalCatMap> ProductStageGlobalCatMaps { get; set; }
        public virtual DbSet<ProductStageGroup> ProductStageGroups { get; set; }
        public virtual DbSet<ProductStageImage> ProductStageImages { get; set; }
        public virtual DbSet<ProductStageLocalCatMap> ProductStageLocalCatMaps { get; set; }
        public virtual DbSet<ProductStageMaster> ProductStageMasters { get; set; }
        public virtual DbSet<ProductStageRelated> ProductStageRelateds { get; set; }
        public virtual DbSet<ProductStageTag> ProductStageTags { get; set; }
        public virtual DbSet<ProductStageVideo> ProductStageVideos { get; set; }
        public virtual DbSet<ProductTag> ProductTags { get; set; }
        public virtual DbSet<ProductTmp> ProductTmps { get; set; }
        public virtual DbSet<ProductVideo> ProductVideos { get; set; }
        public virtual DbSet<PromotionBuy1Get1Item> PromotionBuy1Get1Item { get; set; }
        public virtual DbSet<PromotionOnTopCreditCard> PromotionOnTopCreditCards { get; set; }
        public virtual DbSet<PromotionOnTopCreditNumber> PromotionOnTopCreditNumbers { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Shipping> Shippings { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<ShopCommission> ShopCommissions { get; set; }
        public virtual DbSet<ShopImage> ShopImages { get; set; }
        public virtual DbSet<ShopType> ShopTypes { get; set; }
        public virtual DbSet<ShopTypePermissionMap> ShopTypePermissionMaps { get; set; }
        public virtual DbSet<ShopUserGroupMap> ShopUserGroupMaps { get; set; }
        public virtual DbSet<StoreBranch> StoreBranches { get; set; }
        public virtual DbSet<StoreReceive> StoreReceives { get; set; }
        public virtual DbSet<StoreReceiveCode> StoreReceiveCodes { get; set; }
        public virtual DbSet<StoreReturn> StoreReturns { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<TableA> TableAs { get; set; }
        public virtual DbSet<TableAB> TableABs { get; set; }
        public virtual DbSet<TableB> TableBs { get; set; }
        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserBrandMap> UserBrandMaps { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<UserGroupMap> UserGroupMaps { get; set; }
        public virtual DbSet<UserGroupPermissionMap> UserGroupPermissionMaps { get; set; }
        public virtual DbSet<UserShopMap> UserShopMaps { get; set; }
        public virtual DbSet<Guest> Guests { get; set; }
        public virtual DbSet<Migrate_TBProduct> Migrate_TBProduct { get; set; }
        public virtual DbSet<Migrate_TBProductApproved> Migrate_TBProductApproved { get; set; }
        public virtual DbSet<Migrate_TBProductBundle> Migrate_TBProductBundle { get; set; }
        public virtual DbSet<Migrate_TBProductGroup> Migrate_TBProductGroup { get; set; }
        public virtual DbSet<Migrate_TBProductMaster> Migrate_TBProductMaster { get; set; }
        public virtual DbSet<Migrate_TBProductPicture> Migrate_TBProductPicture { get; set; }
        public virtual DbSet<Migrate_TBProductRelateLink> Migrate_TBProductRelateLink { get; set; }
        public virtual DbSet<Migrate_TBProductVideoLink> Migrate_TBProductVideoLink { get; set; }
        public virtual DbSet<Migrate_TBProperty> Migrate_TBProperty { get; set; }
        public virtual DbSet<Migrate_TBPropertyCategory> Migrate_TBPropertyCategory { get; set; }
        public virtual DbSet<Migrate_TBPropertyFilter> Migrate_TBPropertyFilter { get; set; }
        public virtual DbSet<Migrate_TBPropertyGroup> Migrate_TBPropertyGroup { get; set; }
        public virtual DbSet<Migrate_TBPropertyProduct> Migrate_TBPropertyProduct { get; set; }
        public virtual DbSet<ODMRoleUser> ODMRoleUsers { get; set; }
        public virtual DbSet<PostCodeMap> PostCodeMaps { get; set; }
        public virtual DbSet<StoreReturnReason> StoreReturnReasons { get; set; }
        public virtual DbSet<TBAdminMenuItemTmp> TBAdminMenuItemTmps { get; set; }
        public virtual DbSet<TBCMCity> TBCMCities { get; set; }
        public virtual DbSet<TBDvMapZone> TBDvMapZones { get; set; }
        public virtual DbSet<TBPermissionTmp> TBPermissionTmps { get; set; }
        public virtual DbSet<TBRolePermissionTmp> TBRolePermissionTmps { get; set; }
        public virtual DbSet<TBRoleTmp> TBRoleTmps { get; set; }
        public virtual DbSet<TBRoleUserTmp> TBRoleUserTmps { get; set; }
        public virtual DbSet<TBUserAdminTmp> TBUserAdminTmps { get; set; }
        public virtual DbSet<TBUserAdminWebTokenTmp> TBUserAdminWebTokenTmps { get; set; }
    
        public virtual ObjectResult<Nullable<int>> AttributeValueId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("AttributeValueId");
        }
    
        public virtual ObjectResult<Nullable<long>> GetNextProductStageGroupId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<long>>("GetNextProductStageGroupId");
        }
    
        public virtual ObjectResult<Nullable<int>> GlobalCategoryId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GlobalCategoryId");
        }
    
        public virtual ObjectResult<Nullable<int>> LocalCategoryId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("LocalCategoryId");
        }
    
        public virtual ObjectResult<Nullable<long>> GetNextProductTempId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<long>>("GetNextProductTempId");
        }
    
        public virtual ObjectResult<Nullable<long>> GetNextProductHistoryId()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<long>>("GetNextProductHistoryId");
        }
    }
}
