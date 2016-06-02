
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

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row                         = -1;
                    DateTime dateNow                = DateTime.Now;

                    CMSCategory cmsCategory         = new CMSCategory();
                    cmsCategory.CMSCategoryNameEN   = request.CMSCategoryNameEN;
                    cmsCategory.CMSCategoryNameTH   = request.CMSCategoryNameTH;
                    cmsCategory.Visibility          = request.Visibility;
                    cmsCategory.Status              = request.Status;
                    cmsCategory.CreateBy            = request.CreateBy;
                    cmsCategory.CreateOn            = dateNow;
                    cmsCategory.CreateIP            = request.CreateIP;
                    cmsCategory.UpdateBy            = "";
                    db.CMSCategories.Add(cmsCategory);
                    db.SaveChanges();

                    int? cmsCategoryId = CMSHelper.GetCMSCategoryId(db, cmsCategory);

                    if (cmsCategoryId != null)
                    {

                        foreach (var product in request.CategoryProductList)
                        {
                            CMSCategoryProductMap cmsCategoryProduct    = new CMSCategoryProductMap();
                            cmsCategoryProduct.ShopId                   = request.ShopId;
                            cmsCategoryProduct.CMSCategoryId            = cmsCategoryId.Value;
                            cmsCategoryProduct.CMSCategoryProductMapId  = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.Status                   = product.Status;
                            cmsCategoryProduct.ProductBoxBadge          = product.ProductBoxBadge;
                            cmsCategoryProduct.Pid                      = product.Pid;
                            cmsCategoryProduct.Sequence                 = product.Sequence;
                            cmsCategoryProduct.CreateBy                 = request.CreateBy;
                            cmsCategoryProduct.CreateOn                 = dateNow;
                            cmsCategoryProduct.CreateIP                 = request.CreateIP;
                            cmsCategoryProduct.UpdateBy                 = "";

                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    }

                    row     = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/AddCMSCategory");
            }

            return success;
        }

        // Edit CMS Category
        public bool EditCMSCategory(CMSCategoryRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                    var queryCMSCategory = db.CMSCategories.Where(x => x.CMSCategoryId == request.CMSCategoryId);

                    if (!queryCMSCategory.Any())
                        return false;

                    var cmsCategory                 = queryCMSCategory.First();
                    cmsCategory.CMSCategoryNameEN   = request.CMSCategoryNameEN;
                    cmsCategory.CMSCategoryNameTH   = request.CMSCategoryNameTH;
                    cmsCategory.Visibility          = request.Visibility;
                    cmsCategory.Status              = request.Status;
                    cmsCategory.UpdateBy            = request.UpdateBy;
                    cmsCategory.UpdateOn            = dateNow;
                    cmsCategory.UpdateIP            = request.UpdateIP;

                    // Remove Category Product
                    var queryCMSCategoryProducts = db.CMSCategoryProductMaps.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId);
                    if (queryCMSCategoryProducts.Any())
                    {
                        foreach (var cmsCategoryProduct in queryCMSCategoryProducts)
                        {
                            db.CMSCategoryProductMaps.Remove(cmsCategoryProduct);
                        }
                    }

                    if (cmsCategory != null)
                    {

                        foreach (var product in request.CategoryProductList)
                        {
                            CMSCategoryProductMap cmsCategoryProduct    = new CMSCategoryProductMap();
                            cmsCategoryProduct.ShopId                   = request.ShopId;
                            cmsCategoryProduct.CMSCategoryId            = cmsCategory.CMSCategoryId;
                            cmsCategoryProduct.CMSCategoryProductMapId  = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.Status                   = "AT";
                            cmsCategoryProduct.ProductBoxBadge          = product.ProductBoxBadge;
                            cmsCategoryProduct.Pid                      = product.Pid;
                            cmsCategoryProduct.Sequence                 = product.Sequence;
                            cmsCategoryProduct.CreateBy                 = request.CreateBy;
                            cmsCategoryProduct.CreateOn                 = dateNow;
                            cmsCategoryProduct.CreateIP                 = request.CreateIP;

                            db.CMSCategoryProductMaps.Add(cmsCategoryProduct);
                        }
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/EditCMSCategory");
            }

            return success;
        }

        // Delete CMS Category
        public bool DeleteCMSCategory(List<CMSCategoryRequest> request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    foreach (var cmsCategory in request)
                    {
                        var query = db.CMSCategories.Where(x => x.CMSCategoryId == cmsCategory.CMSCategoryId);
                        if (query.Any())
                            query.First().Status = "RM";
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/DeleteCMSCategory");
            }

            return success;
        }
        #endregion


        #region Start CMS Master Method
        // Add CMS Master
        public bool AddCMSMaster(CMSMasterRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    using (var dbcxtransaction = db.Database.BeginTransaction())
                    {
                        int row                 = -1;
                        DateTime dateNow        = DateTime.Now;
                        DateTime EffectiveDate  = new DateTime();
                        DateTime ExpiryDate     = new DateTime();

                        if (request.EffectiveDate != null)
                        {
                            if (!DateTime.TryParse(request.EffectiveDate.ToString(), out EffectiveDate))
                            {
                                dbcxtransaction.Rollback();
                            }

                        }

                        if (request.ExpiryDate!=null)
                        {
                            if (!DateTime.TryParse(request.ExpiryDate.ToString(), out ExpiryDate))
                            {
                                dbcxtransaction.Rollback();
                            }
                        }

                        CMSMaster cms               = new CMSMaster();
                        cms.CMSMasterNameEN         = request.CMSMasterNameEN;
                        cms.CMSMasterNameTH         = request.CMSMasterNameTH;
                        cms.CMSMasterType           = request.CMSMasterType;
                        cms.CMSMasterEffectiveDate  = request.EffectiveDate;
                        //cms.CMSMasterEffectiveTime  = request.EffectiveTime;
                        cms.CMSMasterExpireDate     = request.ExpiryDate;
                        //cms.CMSMasterExpiryTime     = request.ExpiryTime;
                        cms.LongDescriptionEN       = request.LongDescriptionEN;
                        cms.LongDescriptionTH       = request.LongDescriptionTH;
                        cms.ShortDescriptionEN      = request.ShortDescriptionEN;
                        cms.ShortDescriptionTH      = request.ShortDescriptionTH;
                        cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                        cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                        cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                        cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                        cms.Status                  = request.Status;
                        cms.CMSMasterURLKey         = request.CMSMasterURLKey;
                        cms.Visibility              = request.Visibility;
                        cms.CreateBy                = request.CreateBy;
                        cms.CreateOn                = dateNow;
                        cms.CreateIP                = request.CreateIP;
                        cms.IsCampaign              = request.ISCampaign;
                        cms.FeatureTitle            = request.FeatureTitle;
                        cms.TitleShowcase           = request.TitleShowcase;
                        db.CMSMasters.Add(cms);

                        
                        if (db.SaveChanges() > 0)
                        {
                            dbcxtransaction.Commit();
                        }
                        
                        var masterId = cms.CMSMasterId;

                        // Add cms banner
                        if (request.CMSBannerEN != null && request.CMSBannerEN.Count > 0)
                        {
                            int position = 0;
                            foreach (ImageRequest img in request.CMSBannerEN)
                            {
                                db.CMSImages.Add(new CMSImage()
                                {
                                    ImageUrl  = img.Url,
                                    Position  = position++,
                                    EnTh      = "EN",
                                    CreateBy  = request.CreateBy,
                                    CreateOn  = request.CreateOn.Value
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
                                    ImageUrl  = img.Url,
                                    Position  = position++,
                                    EnTh      = "TH",
                                    CreateBy  = request.CreateBy,
                                    CreateOn  = request.CreateOn.Value
                                });
                            }
                        }

                        // Create Feature Product
                        foreach (var featureProductRq in request.FeatureProductList)
                        {
                            CMSFeatureProduct cmsFeatureProduct = new CMSFeatureProduct();
                            cmsFeatureProduct.CMSMasterId       = masterId;
                            cmsFeatureProduct.ProductId         = featureProductRq.ProductId;
                            cmsFeatureProduct.CreateBy          = featureProductRq.CreateBy;
                            cmsFeatureProduct.CreateOn          = featureProductRq.CreateOn.Value;

                            db.CMSFeatureProducts.Add(cmsFeatureProduct);
                        }

                        // mapping master category
                        foreach (var masterCateRq in request.CategoryList)
                        {
                            CMSMasterCategoryMap cmsMasterCate  = new CMSMasterCategoryMap();
                            cmsMasterCate.CMSCategoryId         = masterCateRq.CMSCategoryId;
                            cmsMasterCate.CMSMasterId           = masterId;
                            cmsMasterCate.Sequence              = masterCateRq.Sequence;
                            cmsMasterCate.ShopId                = masterCateRq.ShopId;
                            cmsMasterCate.Status                = masterCateRq.Status;
                            cmsMasterCate.CreateBy              = request.CreateBy;
                            cmsMasterCate.CreateOn              = request.CreateOn.Value;
                            cmsMasterCate.CreateIP              = request.CreateIP;

                            db.CMSMasterCategoryMaps.Add(cmsMasterCate);
                        }

                        // Create Schedule
                        foreach (var scheduleRq in request.ScheduleList)
                        {
                            CMSScheduler cmsScheduler   = new CMSScheduler();
                            DateTime SchEffectiveDate   = new DateTime();
                            DateTime SchExpiryDate      = new DateTime();

                            if (scheduleRq.EffectiveDate != null)
                            {
                                if (!DateTime.TryParse(scheduleRq.EffectiveDate.ToString(), out SchEffectiveDate))
                                {
                                    dbcxtransaction.Rollback();
                                }

                            }
                            if (scheduleRq.ExpiryDate != null)
                            {
                                if (!DateTime.TryParse(scheduleRq.ExpiryDate.ToString(), out SchExpiryDate))
                                {
                                    dbcxtransaction.Rollback();
                                }
                            }

                            cmsScheduler.EffectiveDate  = SchEffectiveDate.Date;
                            //cmsScheduler.EffectiveTime  = SchEffectiveDate.TimeOfDay;
                            cmsScheduler.ExpireDate     = SchExpiryDate;
                            //cmsScheduler.ExpiryTime     = SchExpiryDate.TimeOfDay;
                            db.CMSSchedulers.Add(cmsScheduler);
                            
                            if (db.SaveChanges() > 0)
                            {
                                var scheduleId = cmsScheduler.CMSSchedulerId;

                                // Mapping Master Schedule
                                CMSMasterSchedulerMap cmsMasterScheduleMap  = new CMSMasterSchedulerMap();
                                cmsMasterScheduleMap.CMSMasterId            = masterId;
                                cmsMasterScheduleMap.CMSSchedulerId         = scheduleId;
                                cmsMasterScheduleMap.Status                 = "AT";
                                db.CMSMasterSchedulerMaps.Add(cmsMasterScheduleMap);
                                
                            }
                        }
                        
                        row = db.SaveChanges();
                        success = row > -1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/AddCMSCategory");
            }

            return success;
        }

        // Edit CMS Master
        public bool EditCMSMaster(CMSMasterRequest request, int shopId, string updateBy)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                    var queryCMSMaster = db.CMSMasters.Where(x => x.CMSMasterId == request.CMSMasterId);

                    if (!queryCMSMaster.Any())
                        return false;

                    // Update Information
                    var cms                     = queryCMSMaster.First();
                    cms.CMSMasterNameEN         = request.CMSMasterNameEN;
                    cms.CMSMasterNameTH         = request.CMSMasterNameTH;
                    cms.CMSMasterType           = request.CMSMasterType;
                    cms.CMSMasterEffectiveDate  = request.EffectiveDate;
                    //cms.CMSMasterEffectiveTime  = request.EffectiveTime;
                    cms.CMSMasterExpireDate     = request.ExpiryDate;
                    //cms.CMSMasterExpiryTime     = request.ExpiryTime;
                    cms.LongDescriptionEN       = request.LongDescriptionEN;
                    cms.LongDescriptionTH       = request.LongDescriptionTH;
                    cms.ShortDescriptionEN      = request.ShortDescriptionEN;
                    cms.ShortDescriptionTH      = request.ShortDescriptionTH;
                    cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                    cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                    cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                    cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                    cms.Status                  = request.Status;
                    cms.CMSMasterURLKey         = request.CMSMasterURLKey;
                    cms.Visibility              = request.Visibility;
                    cms.UpdateBy                = request.UpdateBy;
                    cms.UpdateOn                = dateNow;
                    cms.UpdateIP                = request.UpdateIP;
                    cms.IsCampaign              = request.ISCampaign;
                    cms.FeatureTitle            = request.FeatureTitle;
                    cms.TitleShowcase           = request.TitleShowcase;

                    // Remove Feature Product Item in CMSFeatureProducts
                    var CMSFeatureProductList = db.CMSFeatureProducts.Where(x => x.CMSMasterId == cms.CMSMasterId).ToList();
                    if (CMSFeatureProductList != null && CMSFeatureProductList.Count > 0)
                    {
                        db.CMSFeatureProducts.RemoveRange(CMSFeatureProductList);
                    }

                    // Add Feature Product
                    foreach (var featureProduct in request.FeatureProductList)
                    {
                        CMSFeatureProduct cmsFeatureProduct = new CMSFeatureProduct();
                        cmsFeatureProduct.CMSMasterId       = request.CMSMasterId;
                        cmsFeatureProduct.ProductId         = featureProduct.ProductId;
                        cmsFeatureProduct.UpdateBy          = updateBy;
                        cmsFeatureProduct.UpdateOn          = dateNow;

                        db.CMSFeatureProducts.Add(cmsFeatureProduct);
                    }

                    // Remove Category Map
                    var queryCMSMaterCategoryList = db.CMSMasterCategoryMaps.Where(x => x.CMSMasterId == request.CMSMasterId).ToList();
                    foreach (var masterCate in queryCMSMaterCategoryList)
                    {
                        db.CMSMasterCategoryMaps.Remove(masterCate);
                    }

                    // Map Category
                    foreach (var categoryRq in request.CategoryList)
                    {
                        CMSMasterCategoryMap cmsCategory = new CMSMasterCategoryMap();
                        cmsCategory.CMSCategoryId   = categoryRq.CMSCategoryId;
                        cmsCategory.CMSMasterId     = request.CMSMasterId;
                        cmsCategory.Sequence        = categoryRq.Sequence;
                        cmsCategory.ShopId          = categoryRq.ShopId;
                        cmsCategory.Status          = categoryRq.Status;
                        
                        cmsCategory.UpdateBy        = updateBy;
                        cmsCategory.UpdateOn        = dateNow;

                        db.CMSMasterCategoryMaps.Add(cmsCategory);
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
                            }
                        }

                        var querySchedule = db.CMSSchedulers.Where(x => x.CMSSchedulerId == scheduleRq.CMSSchedulerId).FirstOrDefault();
                        if (querySchedule !=  null)
                        {
                            // Update Schedule
                            querySchedule.EffectiveDate = scheduleRq.EffectiveDate;
                            querySchedule.ExpireDate    = scheduleRq.ExpiryDate;
                            querySchedule.Status        = scheduleRq.Status;
                            querySchedule.UpdateBy      = scheduleRq.UpdateBy;
                            querySchedule.UpdateOn      = scheduleRq.UpdateDate;
                            querySchedule.UpdateIP      = scheduleRq.UpdateIP;

                            // New Map Master Schedule
                            CMSMasterSchedulerMap cmsMasterScheduleMap  = new CMSMasterSchedulerMap();
                            cmsMasterScheduleMap.CMSMasterId            = cms.CMSMasterId;
                            cmsMasterScheduleMap.CMSSchedulerId         = querySchedule.CMSSchedulerId;
                            cmsMasterScheduleMap.Status                 = scheduleRq.Status;
                            db.CMSMasterSchedulerMaps.Add(cmsMasterScheduleMap);
                            
                        }
                        
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/EditCMSCategory");
            }

            return success;
        }

        #endregion

        #region Start CMS Group Method

        // Add CMS Group
        public bool AddCMSGroup(CMSGroupRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                    CMSGroup cmsGroup       = new CMSGroup();
                    cmsGroup.CMSGroupNameEN = request.CMSGroupNameEN;
                    cmsGroup.CMSGroupNameTH = request.CMSGroupNameTH;
                    cmsGroup.Status         = request.Status;
                    cmsGroup.Visibility     = request.Visibility;
                    cmsGroup.CreateBy       = request.CreateBy;
                    cmsGroup.CreateOn       = dateNow;
                    cmsGroup.CreateIP       = request.CreateIP;
                    db.CMSGroups.Add(cmsGroup);
                    db.SaveChanges();

                    var cmsGroupId = cmsGroup.CMSGroupId;

                    foreach (var master in request.GroupMasterList)
                    {
                        CMSMasterGroupMap cmsMasterGroup = new CMSMasterGroupMap();
                        cmsMasterGroup.CMSMasterId  = master.CMSMasterId.Value;
                        cmsMasterGroup.CMSGroupId   = cmsGroupId;
                        cmsMasterGroup.Sequence     = master.Sequence.Value;
                        cmsMasterGroup.CreateBy     = master.CreateBy;
                        cmsMasterGroup.CreateOn     = dateNow;
                        cmsMasterGroup.CreateIP     = master.CreateIP;
                        cmsMasterGroup.Status       = master.Status;
                        db.CMSMasterGroupMaps.Add(cmsMasterGroup);
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/AddCMSGroup");
            }

            return success;
        }

        // Edit CMS Group
        public bool EditCMSGroup(CMSGroupRequest request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    int row = -1;
                    DateTime dateNow = DateTime.Now;

                    var queryCMSGroup = db.CMSGroups.Where(x => x.CMSGroupId == request.CMSGroupId);

                    if (!queryCMSGroup.Any())
                        return false;

                    var cmsGroup            = queryCMSGroup.First();
                    cmsGroup.CMSGroupNameEN = request.CMSGroupNameEN;
                    cmsGroup.CMSGroupNameTH = request.CMSGroupNameTH;
                    cmsGroup.Status         = request.Status;
                    cmsGroup.Visibility     = request.Visibility;
                    cmsGroup.UpdateBy       = request.UpdateBy;
                    cmsGroup.UpdateOn       = dateNow;
                    cmsGroup.UpdateIP       = request.UpdateIP;

                    // Remove Master Group
                    var queryCMSMasterGroups = db.CMSMasterGroupMaps.Where(x => x.CMSGroupId == cmsGroup.CMSGroupId);
                    if (queryCMSMasterGroups.Any())
                    {
                        foreach (var queryCMSMasterGroup in queryCMSMasterGroups)
                        {
                            db.CMSMasterGroupMaps.Remove(queryCMSMasterGroup);
                        }
                    }

                    if (cmsGroup != null)
                    {
                        foreach (var master in request.GroupMasterList)
                        {
                            CMSMasterGroupMap cmsMasterGroup    = new CMSMasterGroupMap();
                            cmsMasterGroup.CMSGroupId           = cmsGroup.CMSGroupId;
                            cmsMasterGroup.CMSMasterGroupMapId  = master.CMSMasterGroupMapId;
                            cmsMasterGroup.CMSMasterId          = master.CMSMasterId.Value;
                            cmsMasterGroup.Status               = master.Status;
                            cmsMasterGroup.Sequence             = master.Sequence.Value;
                            cmsMasterGroup.CreateBy             = master.CreateBy;
                            cmsMasterGroup.CreateOn             = dateNow;
                            cmsMasterGroup.CreateIP             = master.CreateIP;

                            db.CMSMasterGroupMaps.Add(cmsMasterGroup);
                        }
                    }

                    row = db.SaveChanges();
                    success = row > -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/EditCMSGroup");
            }

            return success;
        }

        // Delete CMS Group
        public bool DeleteCMSGroup(List<CMSGroupRequest> request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    foreach (var cmsGroup in request)
                    {
                        var query = db.CMSGroups.Where(x => x.CMSGroupId == cmsGroup.CMSGroupId);
                        if (query.Any())
                            query.First().Status = "RM";
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/DeleteCMSGroup");
            }

            return success;
        }

        public bool DeleteCMSMaster(List<CMSMasterRequest> request)
        {
            bool success = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    foreach (var cmsMaster in request)
                    {
                        var query = db.CMSMasters.Where(x => x.CMSMasterId == cmsMaster.CMSMasterId);
                        if (query.Any())
                            query.First().Status = "RM";
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /Logic/DeleteCMSMaster");
            }

            return success;
        }

        #endregion
    }
}
