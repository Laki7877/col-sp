
using Colsp.Entity.Models;
using Colsp.Model;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using System.Net;
using System.Net.Http;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class CMSLogic
    {

        #region Start CMS Category Method

        // Get All CMS Category
        //public PaginatedResponse GetAllCMSCategory(PaginatedRequest request)
        //{
        //    try
        //    {
        //        using (ColspEntities db = new ColspEntities())
        //        {
        //            var query = from cate in db.CMSCategories select cate;

        //            var total = query.Count();
        //            var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message + " /Logic/GetAllCMSCategory");
        //    }

        //}

        // Add CMS Category

        // Get Brand

        public List<Brand> GetBrand(BrandCondition condition)
        {
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    var query = from brand in db.Brands select brand;

                    if (condition.BrandId != null)
                        query = query.Where(x => x.BrandId == condition.BrandId);

                    if (condition.BrandNameEn != null)
                        query = query.Where(x => x.BrandNameEn.Equals(condition.BrandNameEn));

                    if (condition.BrandNameTh != null)
                        query = query.Where(x => x.BrandNameTh.Equals(condition.BrandNameTh));

                    if (!query.Any())
                        return null;

                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/GetBrand");
            }
        }

        // Add CMS Category
        public bool AddCMSCategory(CMSCategoryRequest request)
        {
            bool success = false;
                using (ColspEntities db = new ColspEntities())
                {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        int row = -1;
                        DateTime dateNow = DateTime.Now;

                        CMSCategory cmsCategory = new CMSCategory();
                        cmsCategory.CMSCategoryNameEN = request.CMSCategoryNameEN;
                        cmsCategory.CMSCategoryNameTH = request.CMSCategoryNameTH;
                        cmsCategory.Visibility = request.Visibility;
                        cmsCategory.Status = request.Status;
                        cmsCategory.CreateBy = request.CreateBy;
                        cmsCategory.CreateOn = dateNow;
                        cmsCategory.CreateIP = request.CreateIP;
                        cmsCategory.UpdateBy = request.CreateBy;
                        cmsCategory.UpdateOn = dateNow;
                        cmsCategory.UpdateIP = request.CreateIP;
                    db.CMSCategories.Add(cmsCategory);
                    db.SaveChanges();

                    int? cmsCategoryId = CMSHelper.GetCMSCategoryId(db, cmsCategory);

                    if (cmsCategoryId != null)
                    {
                        foreach (var product in request.CategoryProductList)
                        {
                                CMSCategoryProductMap cmsCategoryProduct = new CMSCategoryProductMap();
                                cmsCategoryProduct.CMSCategoryId = cmsCategoryId.Value;
                                cmsCategoryProduct.Status = product.Status;
                                if (!string.IsNullOrWhiteSpace(product.ProductBoxBadge))
                                    cmsCategoryProduct.ProductBoxBadge = product.ProductBoxBadge;
                                else
                                    cmsCategoryProduct.ProductBoxBadge = "";
                                cmsCategoryProduct.Pid = product.Pid;
                                cmsCategoryProduct.ShopId = request.ShopId;
                                cmsCategoryProduct.Sequence = product.Sequence;
                                cmsCategoryProduct.CreateBy = cmsCategory.CreateBy;
                                cmsCategoryProduct.CreateOn = dateNow;
                                cmsCategoryProduct.CreateIP = cmsCategory.CreateIP;
                                cmsCategoryProduct.Status = "";
                                cmsCategoryProduct.UpdateBy = cmsCategory.CreateBy;
                                cmsCategoryProduct.UpdateOn = dateNow;
                                cmsCategoryProduct.UpdateIP = cmsCategory.CreateIP;
                                cmsCategoryProduct.Visibility = true;
                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    }

                        row = db.SaveChanges();
                    success = row > -1;
                        if (success == true)
                        {
                            dbcxtransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/AddCMSCategory");

                    }
                }
            }
            return success;
        }

        // Edit CMS Category
        public bool EditCMSCategory(CMSCategoryRequest request)
        {
            bool success = false;

                using (ColspEntities db = new ColspEntities())
                {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                        var queryCMSCategory = db.CMSCategories.Where(x => x.CMSCategoryId == request.CMSCategoryId).FirstOrDefault();

                        if (queryCMSCategory == null)
                        return false;

                        var cmsCategory = queryCMSCategory;
                        cmsCategory.CMSCategoryNameEN = request.CMSCategoryNameEN;
                        cmsCategory.CMSCategoryNameTH = request.CMSCategoryNameTH;
                        cmsCategory.Visibility = request.Visibility;
                        cmsCategory.Status = request.Status;
                        cmsCategory.CreateBy = request.CreateBy;
                        cmsCategory.CreateOn = dateNow;
                        cmsCategory.CreateIP = request.CreateIP;
                        cmsCategory.UpdateBy = request.CreateBy;
                        cmsCategory.UpdateOn = dateNow;
                        cmsCategory.UpdateIP = request.CreateIP;
                        db.Entry(cmsCategory).State = EntityState.Modified;
                        db.SaveChanges();
                    // Remove Category Product
                        var queryCMSCategoryProducts = db.CMSCategoryProductMaps.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId).ToList();
                        if (queryCMSCategoryProducts.Count > 0)
                    {
                        foreach (var cmsCategoryProduct in queryCMSCategoryProducts)
                        {
                            db.CMSCategoryProductMaps.Remove(cmsCategoryProduct);
                                db.SaveChanges();
                        }
                    }

                    if (cmsCategory != null)
                    {
                        foreach (var product in request.CategoryProductList)
                        {
                                CMSCategoryProductMap cmsCategoryProduct = new CMSCategoryProductMap();
                                cmsCategoryProduct.CMSCategoryId = cmsCategory.CMSCategoryId;
                                cmsCategoryProduct.Status = product.Status;
                                if (!string.IsNullOrWhiteSpace(product.ProductBoxBadge))
                                    cmsCategoryProduct.ProductBoxBadge = product.ProductBoxBadge;
                                else
                                    cmsCategoryProduct.ProductBoxBadge = "";
                                cmsCategoryProduct.Pid = product.Pid;
                                cmsCategoryProduct.ShopId = request.ShopId;
                                cmsCategoryProduct.Sequence = product.Sequence;
                                cmsCategoryProduct.CreateBy = cmsCategory.CreateBy;
                                cmsCategoryProduct.CreateOn = dateNow;
                                cmsCategoryProduct.CreateIP = cmsCategory.CreateIP;
                                cmsCategoryProduct.Status = "";
                                cmsCategoryProduct.UpdateBy = cmsCategory.CreateBy;
                                cmsCategoryProduct.UpdateOn = dateNow;
                                cmsCategoryProduct.UpdateIP = cmsCategory.CreateIP;
                                cmsCategoryProduct.Visibility = true;

                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    row = db.SaveChanges();
                        }
                    success = row > -1;
                        if (success == true)
                        {
                            dbcxtransaction.Commit();
                }
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/EditCMSCategory");
            }
                }
            return success;
        }
        }

        // Delete CMS Category
        public bool DeleteCMSCategory(List<CMSCategoryRequest> request)
        {
            bool success = false;

                using (ColspEntities db = new ColspEntities())
                {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                    foreach (var cmsCategory in request)
                    {
                            var query = db.CMSCategories.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId).FirstOrDefault();
                            if (query != null)
                            {
                                query.Status = "RM";
                                db.Entry(query).State = EntityState.Modified;
                    }
                }

                        if (db.SaveChanges() > 0)
                            dbcxtransaction.Commit();
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/DeleteCMSCategory");
            }
                }
            }
            return success;
        }
        #endregion


        #region Start CMS Master Method
        // Add CMS Master
        public bool AddCMSMaster(CMSMasterRequest request)
        {
            bool success = false;


                using (ColspEntities db = new ColspEntities())
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                    try
                    {
                        int row = -1;
                        DateTime dateNow = DateTime.Now;
                        DateTime EffectiveDate = new DateTime();
                        DateTime ExpiryDate = new DateTime();

                        if (request.EffectiveDate != null)
                        {
                            if (!DateTime.TryParse(request.EffectiveDate.ToString(), out EffectiveDate))
                            {
                                dbcxtransaction.Rollback();
                            }

                        }

                        if (request.ExpiryDate != null)
                        {
                            if (!DateTime.TryParse(request.ExpiryDate.ToString(), out ExpiryDate))
                            {
                                dbcxtransaction.Rollback();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(request.FeatureTitle))
                            request.FeatureTitle = "";
                        if (string.IsNullOrWhiteSpace(request.CreateIP))
                            request.CreateIP = "";

                        CMSMaster cms = new CMSMaster();
                        cms.CMSMasterNameEN = request.CMSMasterNameEN;
                        cms.CMSMasterNameTH = request.CMSMasterNameTH;
                        cms.CMSMasterURLKey = request.CMSMasterURLKey;
                        cms.CMSMasterType = request.CMSMasterType;
                        cms.CMSMasterEffectiveDate = request.EffectiveDate;
                        cms.CMSMasterExpireDate = request.ExpiryDate;
                        cms.CMSMasterTotal = 0;
                        cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                        cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                        cms.ShortDescriptionTH = request.ShortDescriptionTH;
                        cms.LongDescriptionTH = request.LongDescriptionTH;
                        cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                        cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                        cms.SortById = 0;
                        cms.ShortDescriptionEN = request.ShortDescriptionEN;
                        cms.LongDescriptionEN = request.LongDescriptionEN;
                        cms.LinkToOutside = "";
                        cms.IsCampaign = request.ISCampaign;
                        cms.MinPrice = 0;
                        cms.MaxPrice = 0;
                        cms.FeatureTitle = request.FeatureTitle;
                        cms.TitleShowcase = request.TitleShowcase;
                        cms.Status = request.Status;
                        cms.Visibility = request.Visibility;
                        cms.CreateBy = request.CreateBy;
                        cms.CreateOn = dateNow;
                        cms.UpdateBy = request.CreateBy;
                        cms.UpdateOn = dateNow;
                        cms.CreateIP = request.CreateIP;
                        cms.UpdateIP = request.CreateIP;
                        db.CMSMasters.Add(cms);

                        
                        if (db.SaveChanges() > 0)
                        {
                        


                        var masterId = cms.CMSMasterId;

                        // Add cms banner
                        if (request.CMSBannerEN != null && request.CMSBannerEN.Count > 0)
                        {
                            int position = 0;
                            foreach (ImageRequest img in request.CMSBannerEN)
                            {
                                db.CMSImages.Add(new CMSImage()
                                {
                                        CMSMasterId = cms.CMSMasterId,
                                        ImageUrl = img.Url,
                                        Position = position++,
                                        EnTh = "EN",
                                        CreateBy = request.CreateBy,
                                        CreateOn = dateNow,
                                        UpdateBy = request.CreateBy,
                                        UpdatedOn = dateNow
                                });
                            }
                        }

                        if (request.CMSBannerTH != null && request.CMSBannerTH.Count > 0)
                        {
                            int position = 0;
                            foreach (ImageRequest img in request.CMSBannerTH)
                            {
                                db.CMSImages.Add(new CMSImage()
                                {
                                        CMSMasterId = cms.CMSMasterId,
                                        ImageUrl = img.Url,
                                        Position = position++,
                                        EnTh = "TH",
                                        CreateBy = request.CreateBy,
                                        CreateOn = dateNow,
                                        UpdateBy = request.CreateBy,
                                        UpdatedOn = dateNow
                                });
                            }
                        }

                        // Create Feature Product
                        foreach (var featureProductRq in request.FeatureProductList)
                        {
                            CMSFeatureProduct cmsFeatureProduct = new CMSFeatureProduct();
                                cmsFeatureProduct.CMSMasterId = masterId;
                                cmsFeatureProduct.ProductId = featureProductRq.ProductId;
                                cmsFeatureProduct.CreateBy = request.CreateBy;
                                cmsFeatureProduct.CreateOn = dateNow;
                                cmsFeatureProduct.UpdateBy = request.CreateBy;
                                cmsFeatureProduct.UpdateOn = dateNow;
                            db.CMSFeatureProducts.Add(cmsFeatureProduct);
                        }

                        // mapping master category
                        foreach (var masterCateRq in request.CategoryList)
                        {
                                CMSMasterCategoryMap cmsMasterCate = new CMSMasterCategoryMap();
                                cmsMasterCate.CMSCategoryId = masterCateRq.CMSCategoryId;
                                cmsMasterCate.CMSMasterId = masterId;
                                cmsMasterCate.Sequence = masterCateRq.Sequence;
                                cmsMasterCate.ShopId = masterCateRq.ShopId;
                                cmsMasterCate.Status = "AT";
                                cmsMasterCate.CreateBy = request.CreateBy;
                                cmsMasterCate.CreateOn = dateNow;
                                cmsMasterCate.CreateIP = request.CreateIP;
                                cmsMasterCate.UpdateBy = request.CreateBy;
                                cmsMasterCate.UpdateOn = dateNow;
                                cmsMasterCate.UpdateIP = request.CreateIP;
                            db.CMSMasterCategoryMaps.Add(cmsMasterCate);
                        }

                        // Create Schedule
                        foreach (var scheduleRq in request.ScheduleList)
                        {
                                CMSScheduler cmsScheduler = new CMSScheduler();
                                DateTime SchEffectiveDate = new DateTime();
                                DateTime SchExpiryDate = new DateTime();

                                if (request.EffectiveDate != null)
                            {
                                    if (!DateTime.TryParse(request.EffectiveDate.ToString(), out SchEffectiveDate))
                                {
                                    dbcxtransaction.Rollback();
                                }

                            }
                                if (request.ExpiryDate != null)
                            {
                                    if (!DateTime.TryParse(request.ExpiryDate.ToString(), out SchExpiryDate))
                                {
                                    dbcxtransaction.Rollback();
                                }
                            }

                                cmsScheduler.EffectiveDate = SchEffectiveDate;
                                cmsScheduler.ExpireDate = SchExpiryDate;
                                cmsScheduler.Status = "AT";
                                cmsScheduler.CreateBy = request.CreateBy;
                                cmsScheduler.CreateOn = dateNow;
                                cmsScheduler.CreateIP = request.CreateIP;
                                cmsScheduler.UpdateBy = request.CreateBy;
                                cmsScheduler.UpdateOn = dateNow;
                                cmsScheduler.UpdateIP = request.CreateIP;
                            db.CMSSchedulers.Add(cmsScheduler);
                            
                            if (db.SaveChanges() > 0)
                            {
                                // Mapping Master Schedule
                                    CMSMasterSchedulerMap cmsMasterScheduleMap = new CMSMasterSchedulerMap();
                                    cmsMasterScheduleMap.CMSMasterId = masterId;
                                    cmsMasterScheduleMap.CMSSchedulerId = cmsScheduler.CMSSchedulerId;
                                    cmsMasterScheduleMap.Status = "AT";
                                    cmsMasterScheduleMap.CreateBy = request.CreateBy;
                                    cmsMasterScheduleMap.CreateOn = dateNow;
                                    cmsMasterScheduleMap.CreateIP = request.CreateIP;
                                    cmsMasterScheduleMap.UpdateBy = request.CreateBy;
                                    cmsMasterScheduleMap.UpdateOn = dateNow;
                                    cmsMasterScheduleMap.UpdateIP = request.CreateIP;
                                db.CMSMasterSchedulerMaps.Add(cmsMasterScheduleMap);
                                }
                            }
                            row = db.SaveChanges();
                        }
                        
                        success = row > -1;

                        if (success == true)
                            dbcxtransaction.Commit();
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/AddCMSCategory");
            }
                }
            }


            return success;
        }

        // Edit CMS Master
        public bool EditCMSMaster(CMSMasterRequest request, int shopId, string updateBy)
        {
            bool success = false;

            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
            try
            {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                        var queryCMSMaster = db.CMSMasters.Where(x => x.CMSMasterId == request.CMSMasterId).FirstOrDefault();

                        if (queryCMSMaster != null)
                        return false;

                    // Update Information
                        var cms = queryCMSMaster;
                        cms.CMSMasterNameEN = request.CMSMasterNameEN;
                        cms.CMSMasterNameTH = request.CMSMasterNameTH;
                        cms.CMSMasterURLKey = request.CMSMasterURLKey;
                        cms.CMSMasterType = request.CMSMasterType;
                        cms.CMSMasterEffectiveDate = request.EffectiveDate;
                        cms.CMSMasterExpireDate = request.ExpiryDate;
                        cms.CMSMasterTotal = 0;
                        cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                    cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                        cms.ShortDescriptionTH = request.ShortDescriptionTH;
                        cms.LongDescriptionTH = request.LongDescriptionTH;
                    cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                        cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                        cms.SortById = 0;
                        cms.ShortDescriptionEN = request.ShortDescriptionEN;
                        cms.LongDescriptionEN = request.LongDescriptionEN;
                        cms.LinkToOutside = "";
                        cms.IsCampaign = request.ISCampaign;
                        cms.MinPrice = 0;
                        cms.MaxPrice = 0;
                        cms.FeatureTitle = request.FeatureTitle;
                        cms.TitleShowcase = request.TitleShowcase;
                        cms.Status = request.Status;
                        cms.Visibility = request.Visibility;
                        cms.CreateBy = cms.CreateBy;
                        cms.CreateOn = cms.CreateOn;
                        cms.CreateIP = cms.CreateIP;
                        cms.UpdateBy = request.CreateBy;
                        cms.UpdateOn = dateNow;
                        cms.UpdateIP = request.CreateIP;
                        db.Entry(cms).State = EntityState.Modified;
                        db.SaveChanges();

                    // Remove Feature Product Item in CMSFeatureProducts
                    var CMSFeatureProductList = db.CMSFeatureProducts.Where(x => x.CMSMasterId == cms.CMSMasterId).ToList();
                    if (CMSFeatureProductList != null && CMSFeatureProductList.Count > 0)
                    {
                        db.CMSFeatureProducts.RemoveRange(CMSFeatureProductList);
                            db.SaveChanges();
                    }

                    // Add Feature Product
                    foreach (var featureProduct in request.FeatureProductList)
                    {
                        CMSFeatureProduct cmsFeatureProduct = new CMSFeatureProduct();
                            cmsFeatureProduct.CMSMasterId = cms.CMSMasterId;
                            cmsFeatureProduct.ProductId = featureProduct.ProductId;
                            cmsFeatureProduct.CreateBy = request.CreateBy;
                            cmsFeatureProduct.CreateOn = dateNow;
                            cmsFeatureProduct.UpdateBy = request.CreateBy;
                            cmsFeatureProduct.UpdateOn = dateNow;

                        db.CMSFeatureProducts.Add(cmsFeatureProduct);
                    }

                    // Remove Category Map
                    var queryCMSMaterCategoryList = db.CMSMasterCategoryMaps.Where(x => x.CMSMasterId == request.CMSMasterId).ToList();
                    foreach (var masterCate in queryCMSMaterCategoryList)
                    {
                        db.CMSMasterCategoryMaps.Remove(masterCate);
                            db.SaveChanges(); 
                    }

                    // Map Category
                    foreach (var categoryRq in request.CategoryList)
                    {
                        CMSMasterCategoryMap cmsCategory = new CMSMasterCategoryMap();
                            cmsCategory.CMSCategoryId = categoryRq.CMSCategoryId;
                            cmsCategory.CMSMasterId = request.CMSMasterId;
                            cmsCategory.Sequence = categoryRq.Sequence;
                            cmsCategory.ShopId = categoryRq.ShopId;
                            cmsCategory.Status = categoryRq.Status;
                            cmsCategory.CreateBy = request.CreateBy;
                            cmsCategory.CreateOn = dateNow;
                            cmsCategory.CreateIP = request.CreateIP;
                            cmsCategory.UpdateBy = request.CreateBy;
                            cmsCategory.UpdateOn = dateNow;
                            cmsCategory.UpdateIP = request.CreateIP;
                        db.CMSMasterCategoryMaps.Add(cmsCategory);
                            db.SaveChanges();
                    }

                    foreach (var scheduleRq in request.ScheduleList)
                    {
                        // Remove Master Schedule Map
                        var queryCMSScheduleList = db.CMSMasterSchedulerMaps.Where(x => x.CMSSchedulerId == scheduleRq.CMSSchedulerId).ToList();

                        if (queryCMSScheduleList != null && queryCMSScheduleList.Count > 0)
                        {
                            foreach (var cmsSchedule in queryCMSScheduleList)
                            {
                                db.CMSMasterSchedulerMaps.Remove(cmsSchedule);
                                    db.SaveChanges();
                            }
                        }

                        var querySchedule = db.CMSSchedulers.Where(x => x.CMSSchedulerId == scheduleRq.CMSSchedulerId).FirstOrDefault();
                            if (querySchedule != null)
                        {
                            // Update Schedule
                            querySchedule.EffectiveDate = scheduleRq.EffectiveDate;
                                querySchedule.ExpireDate = scheduleRq.ExpiryDate;
                                querySchedule.Status = scheduleRq.Status;
                                querySchedule.CreateBy = querySchedule.CreateBy;
                                querySchedule.CreateOn = querySchedule.CreateOn;
                                querySchedule.CreateIP = querySchedule.CreateIP;
                                querySchedule.UpdateBy = scheduleRq.UpdateBy;
                                querySchedule.UpdateOn = scheduleRq.UpdateDate;
                                querySchedule.UpdateIP = scheduleRq.UpdateIP;
                                db.Entry(querySchedule).State = EntityState.Modified;
                            // New Map Master Schedule
                                CMSMasterSchedulerMap cmsMasterScheduleMap = new CMSMasterSchedulerMap();
                                cmsMasterScheduleMap.CMSMasterId = cms.CMSMasterId;
                                cmsMasterScheduleMap.CMSSchedulerId = querySchedule.CMSSchedulerId;
                                cmsMasterScheduleMap.Status = scheduleRq.Status;
                                cmsMasterScheduleMap.CreateBy = request.CreateBy;
                                cmsMasterScheduleMap.CreateOn = dateNow;
                                cmsMasterScheduleMap.CreateIP = request.CreateIP;
                                cmsMasterScheduleMap.UpdateBy = request.CreateBy;
                                cmsMasterScheduleMap.UpdateOn = dateNow;
                                cmsMasterScheduleMap.UpdateIP = request.CreateIP;
                            db.CMSMasterSchedulerMaps.Add(cmsMasterScheduleMap);
                            
                        }
                        
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                        if (success == true)
                            dbcxtransaction.Commit();

            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/EditCMSCategory");
            }
                }
            }
            return success;
        }

        #endregion

        #region Start CMS Group Method

        // Add CMS Group
        public bool AddCMSGroup(CMSGroupRequest request)
        {
            bool success = false;

            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
            try
            {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                        CMSGroup cmsGroup = new CMSGroup();
                    cmsGroup.CMSGroupNameEN = request.CMSGroupNameEN;
                    cmsGroup.CMSGroupNameTH = request.CMSGroupNameTH;
                        cmsGroup.Status = request.Status;
                        cmsGroup.Visibility = request.Visibility;
                        cmsGroup.CreateBy = request.CreateBy;
                        cmsGroup.CreateOn = dateNow;
                        cmsGroup.CreateIP = request.CreateIP;
                        cmsGroup.UpdateBy = request.CreateBy;
                        cmsGroup.UpdateOn = dateNow;
                        cmsGroup.UpdateIP = request.CreateIP;
                    db.CMSGroups.Add(cmsGroup);
                    db.SaveChanges();

                    var cmsGroupId = cmsGroup.CMSGroupId;

                    foreach (var master in request.GroupMasterList)
                    {
                        CMSMasterGroupMap cmsMasterGroup = new CMSMasterGroupMap();
                            cmsMasterGroup.CMSMasterId = master.CMSMasterId.Value;
                            cmsMasterGroup.CMSGroupId = cmsGroupId;
                            cmsMasterGroup.Sequence = master.Sequence.Value;
                            cmsMasterGroup.CreateBy = request.CreateBy;
                            cmsMasterGroup.CreateOn = dateNow;
                            cmsMasterGroup.CreateIP = request.CreateIP;
                            cmsMasterGroup.Status = request.Status;
                            cmsMasterGroup.UpdateBy = request.CreateBy;
                            cmsMasterGroup.UpdateOn = dateNow;
                            cmsMasterGroup.UpdateIP = request.CreateIP;
                            cmsMasterGroup.ShopId = request.ShopId;
                        db.CMSMasterGroupMaps.Add(cmsMasterGroup);
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                        if (success == true)
                            dbcxtransaction.Commit();
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/AddCMSGroup");
            }
                }
            }
            return success;
        }

        // Edit CMS Group
        public bool EditCMSGroup(CMSGroupRequest request)
        {
            bool success = false;

            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
            try
            {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                        var queryCMSGroup = db.CMSGroups.Where(x => x.CMSGroupId == request.CMSGroupId).FirstOrDefault();

                        if (queryCMSGroup == null)
                        return false;

                        var cmsGroup = queryCMSGroup;
                    cmsGroup.CMSGroupNameEN = request.CMSGroupNameEN;
                    cmsGroup.CMSGroupNameTH = request.CMSGroupNameTH;
                        cmsGroup.Status = request.Status;
                        cmsGroup.Visibility = request.Visibility;
                        cmsGroup.UpdateBy = request.UpdateBy;
                        cmsGroup.UpdateOn = dateNow;
                        cmsGroup.UpdateIP = request.UpdateIP;
                        cmsGroup.UpdateBy = request.CreateBy;
                        cmsGroup.UpdateOn = dateNow;
                        cmsGroup.UpdateIP = request.CreateIP;
                        db.Entry(cmsGroup).State = EntityState.Modified;
                        db.SaveChanges();
                    // Remove Master Group
                        var queryCMSMasterGroups = db.CMSMasterGroupMaps.Where(x => x.CMSGroupId == cmsGroup.CMSGroupId).ToList();
                        if (queryCMSMasterGroups.Count() > 0)
                    {
                        foreach (var queryCMSMasterGroup in queryCMSMasterGroups)
                        {
                            db.CMSMasterGroupMaps.Remove(queryCMSMasterGroup);
                                db.SaveChanges();
                        }
                    }

                    if (cmsGroup != null)
                    {
                        foreach (var master in request.GroupMasterList)
                        {
                                CMSMasterGroupMap cmsMasterGroup = new CMSMasterGroupMap();
                                cmsMasterGroup.CMSMasterId = master.CMSMasterId.Value;
                                cmsMasterGroup.CMSGroupId = cmsGroup.CMSGroupId;
                                cmsMasterGroup.Sequence = master.Sequence.Value;
                                cmsMasterGroup.CreateBy = master.CreateBy;
                                cmsMasterGroup.CreateOn = dateNow;
                                cmsMasterGroup.CreateIP = master.CreateIP;
                                cmsMasterGroup.Status = master.Status;
                                cmsMasterGroup.UpdateBy = master.CreateBy;
                                cmsMasterGroup.UpdateOn = dateNow;
                                cmsMasterGroup.UpdateIP = master.CreateIP;
                                cmsMasterGroup.ShopId = request.ShopId;

                            db.CMSMasterGroupMaps.Add(cmsMasterGroup);
                        }
                    row = db.SaveChanges();
                        }
                    success = row > -1;
                        if (success == true)
                            dbcxtransaction.Commit();
            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/EditCMSGroup");
            }
                }
            }
            return success;
        }

        // Delete CMS Group
        public bool DeleteCMSGroup(List<CMSGroupRequest> request)
        {
            bool success = false;

            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
            try
            {
                    foreach (var cmsGroup in request)
                    {
                            var query = db.CMSGroups.Where(x => x.CMSGroupId == cmsGroup.CMSGroupId).FirstOrDefault();
                            if (query != null)
                            {
                                query.Status = "RM";
                                db.Entry(query).State = EntityState.Modified;
                            }
                    }
                    db.SaveChanges();
                        dbcxtransaction.Commit();

            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/DeleteCMSGroup");
            }
                }
            }
            return success;
        }

        public bool DeleteCMSMaster(List<CMSMasterRequest> request)
        {
            bool success = false;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
            try
            {
                    foreach (var cmsMaster in request)
                    {
                            var query = db.CMSMasters.Where(x => x.CMSMasterId == cmsMaster.CMSMasterId).FirstOrDefault();
                            if (query != null)
                            {
                                query.Status = "RM";
                                db.Entry(query).State = EntityState.Modified;
                            }
                    }
                    db.SaveChanges();
                        dbcxtransaction.Commit();

            }
            catch (Exception ex)
            {
                        dbcxtransaction.Rollback();
                throw new Exception(ex.Message + " /Logic/DeleteCMSMaster");
            }
                }
            }
            return success;
        }

        #endregion
    }
}
