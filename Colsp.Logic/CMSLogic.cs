
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
                    cmsCategory.CreateDate          = dateNow;
                    cmsCategory.CreateIP            = request.CreateIP;
                    db.CMSCategories.Add(cmsCategory);
                    db.SaveChanges();

                    int? cmsCategoryId = CMSHelper.GetCMSCategoryId(db, cmsCategory);

                    if (cmsCategoryId != null)
                    {

                        foreach (var product in request.CategoryProductList)
                        {
                            CMSCategoryProductMap cmsCategoryProduct    = new CMSCategoryProductMap();
                            cmsCategoryProduct.CMSCategoryId            = cmsCategoryId.Value;
                            cmsCategoryProduct.CMSCategoryProductMapId  = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.IsActive                 = product.IsActive;
                            cmsCategoryProduct.ProductBoxBadge          = product.ProductBoxBadge;
                            cmsCategoryProduct.Pid                      = product.Pid;
                            cmsCategoryProduct.Sequence                 = product.Sequence;
                            cmsCategoryProduct.CreateBy                 = product.CreateBy;
                            cmsCategoryProduct.CreateDate               = dateNow;
                            cmsCategoryProduct.CreateIP                 = product.CreateIP;

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
                    cmsCategory.UpdateDate          = dateNow;
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
                            cmsCategoryProduct.CMSCategoryId            = cmsCategory.CMSCategoryId;
                            cmsCategoryProduct.CMSCategoryProductMapId  = product.CMSCategoryProductMapId;
                            cmsCategoryProduct.IsActive                 = product.IsActive;
                            cmsCategoryProduct.ProductBoxBadge          = product.ProductBoxBadge;
                            cmsCategoryProduct.Pid                      = product.Pid;
                            cmsCategoryProduct.Sequence                 = product.Sequence;
                            cmsCategoryProduct.CreateBy                 = product.CreateBy;
                            cmsCategoryProduct.CreateDate               = dateNow;
                            cmsCategoryProduct.CreateIP                 = product.CreateIP;

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
                        int row = -1;
                        DateTime dateNow = DateTime.Now;
                        DateTime EffectiveDate = new DateTime();
                        DateTime ExpiryDate = new DateTime();
                        if (request.EffectiveDate!=null)
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
                        cms.CMSTypeId               = request.CMSMasterTypeId;
                        cms.CMSMasterEffectiveDate  = request.EffectiveDate;
                        cms.CMSMasterEffectiveTime  = request.EffectiveTime;
                        cms.CMSMasterExpiryDate     = request.ExpiryDate;
                        cms.CMSMasterExpiryTime     = request.ExpiryTime;
                        cms.LongDescriptionEN       = request.LongDescriptionEN;
                        cms.LongDescriptionTH       = request.LongDescriptionTH;
                        cms.ShortDescriptionEN      = request.ShortDescriptionEN;
                        cms.ShortDescriptionTH      = request.ShortDescriptionTH;
                        cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                        cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                        cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                        cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                        cms.Status                  = request.Status;
                        cms.CMSMasterStatusId       = request.CMSMasterStatusId;
                        cms.Sequence                = request.Sequence;
                        cms.CMSMasterURLKey         = request.CMSMasterURLKey;
                        cms.Visibility              = request.Visibility;
                        cms.CreateBy                = request.CreateBy;
                        cms.Createdate              = dateNow;
                        cms.CreateIP                = request.CreateIP;
                        cms.IsCampaign              = request.ISCampaign;
                        db.CMSMasters.Add(cms);

                        if (db.SaveChanges() > 0)
                        {
                            dbcxtransaction.Commit();
                        }
                        
                        // Create Schedule
                        var masterId = cms.CMSMasterId;
                        foreach (var schedule in request.ScheduleList)
                        {
                            CMSScheduler cmsScheduler   = new CMSScheduler();
                            DateTime SchEffectiveDate   = new DateTime();
                            DateTime SchExpiryDate      = new DateTime();
                            if (schedule.EffectiveDate != null)
                            {
                                if (!DateTime.TryParse(schedule.EffectiveDate.ToString(), out SchEffectiveDate))
                                {
                                    dbcxtransaction.Rollback();
                                }

                            }
                            if (schedule.ExpiryDate != null)
                            {
                                if (!DateTime.TryParse(schedule.ExpiryDate.ToString(), out SchExpiryDate))
                                {
                                    dbcxtransaction.Rollback();
                                }
                            }
                            cmsScheduler.EffectiveDate  = SchEffectiveDate.Date;
                            cmsScheduler.EffectiveTime  = SchEffectiveDate.TimeOfDay;
                            cmsScheduler.ExpiryDate     = SchExpiryDate;
                            cmsScheduler.ExpiryTime     = SchExpiryDate.TimeOfDay;
                            db.CMSSchedulers.Add(cmsScheduler);
                            
                            if (db.SaveChanges() > 0)
                            {
                                var scheduleId = cmsScheduler.CMSSchedulerId;

                                // Map Master Schedule
                                CMSMasterSchedulerMap cmsMasterScheduleMap  = new CMSMasterSchedulerMap();
                                cmsMasterScheduleMap.CMSMasterId            = masterId;
                                cmsMasterScheduleMap.CMSSchedulerId         = scheduleId;
                                cmsMasterScheduleMap.IsActive               = true;
                                db.CMSMasterSchedulerMaps.Add(cmsMasterScheduleMap);
                                
                                foreach (var category in schedule.CategoryList)
                                {
                                    // Map Category Schedule
                                    CMSCategorySchedulerMap cmsCategorySchedulerMap = new CMSCategorySchedulerMap();
                                    cmsCategorySchedulerMap.CMSSchedulerId = scheduleId;
                                    cmsCategorySchedulerMap.CMSCategoryId = category.CMSCategoryId;
                                    cmsCategorySchedulerMap.IsActive = true;
                                    db.CMSCategorySchedulerMaps.Add(cmsCategorySchedulerMap);
                                }
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
        public bool EditCMSMaster(CMSMasterRequest request)
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
                    cms.CMSMasterEffectiveTime  = request.EffectiveTime;
                    cms.CMSMasterExpiryDate     = request.ExpiryDate;
                    cms.CMSMasterExpiryTime     = request.ExpiryTime;
                    cms.LongDescriptionEN       = request.LongDescriptionEN;
                    cms.LongDescriptionTH       = request.LongDescriptionTH;
                    cms.ShortDescriptionEN      = request.ShortDescriptionEN;
                    cms.ShortDescriptionTH      = request.ShortDescriptionTH;
                    cms.MobileLongDescriptionEN = request.MobileLongDescriptionEN;
                    cms.MobileLongDescriptionTH = request.MobileLongDescriptionTH;
                    cms.MobileShortDescriptionEN = request.MobileShortDescriptionEN;
                    cms.MobileShortDescriptionTH = request.MobileShortDescriptionTH;
                    cms.Status                  = request.Status;
                    cms.CMSMasterStatusId       = request.CMSMasterStatusId;
                    cms.Sequence                = request.Sequence;
                    cms.CMSMasterURLKey         = request.CMSMasterURLKey;
                    cms.Visibility              = request.Visibility;
                    cms.UpdateBy                = request.UpdateBy;
                    cms.UpdateDate              = dateNow;
                    cms.UpdateIP                = request.UpdateIP;
                    cms.IsCampaign              = request.ISCampaign;
                    

                    foreach (var schedule in request.ScheduleList)
                    {
                        var querySchedule = db.CMSSchedulers.Where(x => x.CMSSchedulerId == schedule.CMSSchedulerId).FirstOrDefault();
                        if (querySchedule !=  null)
                        {
                            // Update Schedule
                            querySchedule.EffectiveDate = schedule.EffectiveDate;
                            querySchedule.ExpiryDate    = schedule.ExpiryDate;
                            querySchedule.Status        = schedule.Status;
                            querySchedule.Visibility    = schedule.Visibility;
                            querySchedule.UpdateBy      = schedule.UpdateBy;
                            querySchedule.UpdateDate    = schedule.UpdateDate;
                            querySchedule.UpdateIP      = schedule.UpdateIP;
                            
                            // Remove Category Item in CMSCategorySchedulerMaps
                            var queryCategoryScheduleList = db.CMSCategorySchedulerMaps.Where(x => x.CMSSchedulerId == querySchedule.CMSSchedulerId).ToList();
                            if (queryCategoryScheduleList != null && queryCategoryScheduleList.Count > 0)
                            {
                                foreach (var cateSchedule in queryCategoryScheduleList)
                                {
                                    db.CMSCategorySchedulerMaps.Remove(cateSchedule);
                                }
                            }

                            // Add New Category Item To CMSCategorySchedulerMaps
                            foreach (var cate in schedule.CategoryList)
                            {
                                CMSCategorySchedulerMap cmsCategorySchedulerMap = new CMSCategorySchedulerMap();
                                cmsCategorySchedulerMap.CMSSchedulerId          = schedule.CMSSchedulerId;
                                cmsCategorySchedulerMap.CMSCategoryId           = cate.CMSCategoryId;
                                cmsCategorySchedulerMap.IsActive                = true;
                                db.CMSCategorySchedulerMaps.Add(cmsCategorySchedulerMap);
                            }

                            // Update Master Schedule Map
                            //var queryMasterScheduleMap = db.CMSMasterSchedulerMaps.Where(x => x.CMSSchedulerId == schedule.CMSSchedulerId).FirstOrDefault();
                            //if (queryMasterScheduleMap != null)
                            //{
                            //    queryMasterScheduleMap.IsActive = schedule.Status;
                            //}
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
                    cmsGroup.CreateDate     = dateNow;
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
                        cmsMasterGroup.CreateDate   = dateNow;
                        cmsMasterGroup.CreateIP     = master.CreateIP;
                        cmsMasterGroup.IsActive     = master.IsActive;
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
                    cmsGroup.UpdateDate     = dateNow;
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
                            cmsMasterGroup.IsActive             = master.IsActive;
                            cmsMasterGroup.Sequence             = master.Sequence.Value;
                            cmsMasterGroup.CreateBy             = master.CreateBy;
                            cmsMasterGroup.CreateDate           = dateNow;
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
        #endregion
    }
}
