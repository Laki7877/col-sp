using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;


namespace Colsp.Api.CMSFunction
{
    public class CMSProcess
    {
        //Abstract

        #region Create CMS 

        public int CreateCMSMaster(CMSMasterRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSMaster cms = new CMSMaster();
                        cms.CMSMasterNameEN = Model.CMSMasterNameEN;
                        cms.CMSMasterNameTH = Model.CMSMasterNameTH;
                        //cms.CMSTypeId = Model.CMSMasterTypeId;
                        cms.CMSMasterEffectiveDate = Model.EffectiveDate;
                        cms.CMSMasterEffectiveTime = Model.EffectiveTime;
                        cms.CMSMasterExpiryDate = Model.ExpiryDate;
                        cms.CMSMasterExpiryTime = Model.ExpiryTime;
                        cms.LongDescriptionEN = Model.LongDescriptionEN;
                        cms.LongDescriptionTH = Model.LongDescriptionTH;
                        cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                        cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                        //cms.Status = Model.Status;
                        //cms.CMSMasterStatusId = Model.CMSMasterStatusId;
                        //cms.Sequence = Model.Sequence;
                        cms.CMSMasterURLKey = Model.CMSMasterURLKey;
                        //cms.Visibility = Model.Visibility;
                        //cms.CreateBy = Model.CreateBy;
                        //cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        cms.IsCampaign = Model.ISCampaign;
                        db.CMSMasters.Add(cms);

                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            result = cms.CMSMasterId;
                        }


                        // Add Schedule
                        var masterId = cms.CMSMasterId;
                        foreach (var schedule in Model.ScheduleList)
                        {
                            CMSScheduler cmsScheduler = new CMSScheduler();
                            cmsScheduler.EffectiveDate = schedule.EffectiveDate;
                            cmsScheduler.ExpiryDate = schedule.ExpiryDate;
                            db.CMSSchedulers.Add(cmsScheduler);

                            if (db.SaveChanges() > 0)
                            {
                                var scheduleId = cmsScheduler.CMSSchedulerId;
                                foreach (var category in schedule.CategoryList)
                                {
                                    CMSCategorySchedulerMap cmsCategorySchedulerMap = new CMSCategorySchedulerMap();
                                    cmsCategorySchedulerMap.CMSSchedulerId          = scheduleId;
                                    cmsCategorySchedulerMap.CMSCategoryId           = category.CMSCategoryId;
                                    cmsCategorySchedulerMap.Status                  = null;
                                    db.CMSCategorySchedulerMaps.Add(cmsCategorySchedulerMap);
                                }

                                db.SaveChanges();
                            }
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();

                        return result;
                    }
                }
            }
        }

        public CMSMasterAllRequest CMSUpdateStatus(CMSMasterItemListRequest model, int UserId, int? ShopId)
        {
            CMSMasterAllRequest result = new CMSMasterAllRequest();
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    foreach (var item in model.CMSMasterList)
                    {
                        try
                        {
                            var cms = db.CMSMasters.Where(c => c.CMSMasterId == item.CMSMasterId).FirstOrDefault();
                            if (cms != null)
                            {
                                //cms.CMSMasterStatusId = item.CMSMasterStatusId;
                                //cms.CMSMasterStatusId = item.CMSMasterStatusId;
                                //cms.Visibility = item.CMSMasterVisibility;
                                //cms.UpdateBy = UserId;
                                //cms.UpdateDate = DateTime.Now;
                                cms.UpdateIP = model.CreateIP;
                                db.Entry(cms).State = EntityState.Modified;
                                db.SaveChanges();
                                dbcxtransaction.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            dbcxtransaction.Rollback();
                        }
                    }
                }
            }
            result.SearchText = model.SearchText;
            result._direction = model._direction;
            result._filter = model._filter;
            result._limit = model._limit;
            result._offset = model._offset;
            result._order = model._order;
            return result;
        }

        /// <summary>
        /// param as 
        /// </summary>
        public int CreateCMSGroup(CMSGroupRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSGroup cms = new CMSGroup();
                        cms.CMSGroupNameEN = Model.CMSGroupNameEN;
                        cms.CMSGroupNameTH = Model.CMSGroupNameTH;
                        cms.Status = Model.Status;
                        //cms.Sequence = Model.Sequence;
                        cms.Visibility = Model.Visibility;
                        //cms.CreateBy = Model.CreateBy;
                        //cms.CreateDate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        db.CMSGroups.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            result = cms.CMSGroupId;
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();

                        return result;
                    }
                }
            }
        }


        /// <summary>
        /// When not campaign 
        /// </summary>
        /// <param name="Master"></param>
        /// <param name="UserId"></param>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        public int CreateCMSScheduler(CMSMaster Master, int UserId, int? ShopId)
        {
            int result = 0;
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    CMSScheduler sc = new CMSScheduler();
                    //sc.CreateBy = UserId;
                    //sc.Createdate = DateTime.Now;
                    sc.EffectiveDate = Master.CMSMasterEffectiveDate;
                    sc.EffectiveTime = Master.CMSMasterEffectiveTime;
                    sc.ExpiryDate = Master.CMSMasterExpiryDate;
                    sc.ExpiryTime = Master.CMSMasterExpiryTime;
                    //sc.Visibility = Master.Visibility;
                    sc.CreateIP = Master.CreateIP;
                    //sc.Status = Master.Status;
                    db.CMSSchedulers.Add(sc);
                    if (db.SaveChanges() > 0)
                        result = sc.CMSSchedulerId;
                }
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        #region Edit CMS

        public void Edit() { }

        /// <summary>
        /// param as cms class
        /// </summary>
        public int EditCMSStaticPage(CMSCollectionItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cms = db.CMSMasters.Where(c => c.CMSMasterId == Model.CMSId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSMasterNameEN = Model.CMSNameEN;
                            cms.CMSMasterNameTH = Model.CMSNameTH;
                            //cms.CMSTypeId = Model.CMSTypeId;
                            cms.CMSMasterEffectiveDate = Model.EffectiveDate;
                            cms.CMSMasterEffectiveTime = Model.EffectiveTime;
                            cms.CMSMasterExpiryDate = Model.ExpiryDate;
                            cms.CMSMasterExpiryTime = Model.ExpiryTime;
                            cms.LongDescriptionEN = Model.LongDescriptionEN;
                            cms.LongDescriptionTH = Model.LongDescriptionTH;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                            //cms.Status = Model.Status;
                            //cms.CMSMasterStatusId = Model.CMSStatusFlowId;
                            //cms.Sequence = Model.Sequence;
                            cms.CMSMasterURLKey = Model.URLKey;
                            //cms.Visibility = Model.Visibility;
                            //cms.UpdateBy = Model.CreateBy;
                            //cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = cms.CMSMasterId;
                            }
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();
                        return result;
                    }
                }
            }
        }
        public int EditCMSCollectionItem(CMSCollectionItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cms = db.CMSMasters.Where(c => c.CMSMasterId == Model.CMSId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSMasterNameEN = Model.CMSNameEN;
                            cms.CMSMasterNameTH = Model.CMSNameTH;
                            //cms.CMSTypeId = Model.CMSTypeId;
                            cms.CMSMasterEffectiveDate = Model.EffectiveDate;
                            cms.CMSMasterEffectiveTime = Model.EffectiveTime;
                            cms.CMSMasterExpiryDate = Model.ExpiryDate;
                            cms.CMSMasterExpiryTime = Model.ExpiryTime;
                            cms.LongDescriptionEN = Model.LongDescriptionEN;
                            cms.LongDescriptionTH = Model.LongDescriptionTH;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                            //cms.Status = Model.Status;
                            //cms.CMSMasterStatusId = Model.CMSStatusFlowId;
                            //cms.Sequence = Model.Sequence;
                            //cms.CMSMasterURLKey = Model.URLKey;
                            //cms.Visibility = Model.Visibility;
                            //cms.UpdateBy = Model.CreateBy;
                            //cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {


                                dbcxtransaction.Commit();


                                result = cms.CMSMasterId;
                            }
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();
                        return result;
                    }
                }
            }

        }


        //Thanakrit : 20160215 , update only sending field
        public int UpdateCMSMaster(CMSMasterRequest Model)
        {
            var modelItem = Model;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    //foreach (var modelItem in Model) {
                    try
                    {
                        var cms = db.CMSMasters.Where(c => c.CMSMasterId == modelItem.CMSMasterId).FirstOrDefault();
                        if (cms != null)
                        {

                            cms.CMSMasterNameEN = modelItem.CMSMasterNameEN != default(string) ? modelItem.CMSMasterNameEN : cms.CMSMasterNameEN;
                            cms.CMSMasterNameTH = modelItem.CMSMasterNameTH != default(string) ? modelItem.CMSMasterNameTH : cms.CMSMasterNameTH;

                            //cms.CMSTypeId = modelItem.CMSMasterTypeId ?? cms.CMSTypeId;
                            cms.CMSMasterEffectiveDate = modelItem.EffectiveDate ?? cms.CMSMasterEffectiveDate;
                            cms.CMSMasterEffectiveTime = modelItem.EffectiveTime ?? cms.CMSMasterEffectiveTime;
                            cms.CMSMasterExpiryDate = modelItem.ExpiryDate ?? cms.CMSMasterExpiryDate;
                            cms.CMSMasterExpiryTime = modelItem.ExpiryTime ?? cms.CMSMasterExpiryTime;
                            cms.LongDescriptionEN = modelItem.LongDescriptionEN != default(string) ? modelItem.LongDescriptionEN : cms.LongDescriptionEN;
                            cms.LongDescriptionTH = modelItem.LongDescriptionTH != default(string) ? modelItem.LongDescriptionTH : cms.LongDescriptionTH;
                            cms.ShortDescriptionEN = modelItem.ShortDescriptionEN != default(string) ? modelItem.ShortDescriptionEN : cms.ShortDescriptionEN;
                            cms.ShortDescriptionTH = modelItem.ShortDescriptionTH != default(string) ? modelItem.ShortDescriptionTH : cms.ShortDescriptionTH;
                            //cms.CMSCollectionGroupId = modelItem.CMSCollectionGroupId ?? cms.CMSCollectionGroupId;
                            //cms.Status = modelItem.Status ?? cms.Status;
                            //cms.CMSMasterStatusId = modelItem.CMSMasterStatusId ?? cms.CMSMasterStatusId;
                            //cms.Sequence = modelItem.Sequence ?? cms.Sequence;
                            cms.CMSMasterURLKey = modelItem.CMSMasterURLKey != default(string) ? modelItem.CMSMasterURLKey : cms.CMSMasterURLKey;
                            //cms.Visibility = modelItem.Visibility ?? cms.Visibility;
                            //cms.UpdateBy = modelItem.CreateBy ?? cms.UpdateBy;
                            //cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = modelItem.CreateIP != default(string) ? modelItem.CreateIP : cms.UpdateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = cms.CMSMasterId;
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();
                        return result;
                    }
                    //}
                    return result;
                }// end db transaction
            }
        }
        public int UpdateCMSCollectionItem(CMSCollectionItemRequest Model)
        {
            var modelItem = Model;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    //foreach (var modelItem in Model) {
                    try
                    {
                        var cms = db.CMSMasters.Where(c => c.CMSMasterId == modelItem.CMSId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSMasterNameEN = modelItem.CMSNameEN != default(string) ? modelItem.CMSNameEN : cms.CMSMasterNameEN;
                            cms.CMSMasterNameTH = modelItem.CMSNameTH != default(string) ? modelItem.CMSNameTH : cms.CMSMasterNameTH;

                            //cms.CMSTypeId = modelItem.CMSTypeId ?? cms.CMSTypeId;
                            cms.CMSMasterEffectiveDate = modelItem.EffectiveDate ?? cms.CMSMasterEffectiveDate;
                            cms.CMSMasterEffectiveTime = modelItem.EffectiveTime ?? cms.CMSMasterEffectiveTime;
                            cms.CMSMasterExpiryDate = modelItem.ExpiryDate ?? cms.CMSMasterExpiryDate;
                            cms.CMSMasterExpiryTime = modelItem.ExpiryTime ?? cms.CMSMasterExpiryTime;
                            cms.LongDescriptionEN = modelItem.LongDescriptionEN != default(string) ? modelItem.LongDescriptionEN : cms.LongDescriptionEN;
                            cms.LongDescriptionTH = modelItem.LongDescriptionTH != default(string) ? modelItem.LongDescriptionTH : cms.LongDescriptionTH;
                            cms.ShortDescriptionEN = modelItem.ShortDescriptionEN != default(string) ? modelItem.ShortDescriptionEN : cms.ShortDescriptionEN;
                            cms.ShortDescriptionTH = modelItem.ShortDescriptionTH != default(string) ? modelItem.ShortDescriptionTH : cms.ShortDescriptionTH;
                            //cms.CMSCollectionGroupId = modelItem.CMSCollectionGroupId ?? cms.CMSCollectionGroupId;
                            //cms.Status = modelItem.Status ?? cms.Status;
                            //cms.CMSMasterStatusId = modelItem.CMSStatusFlowId ?? cms.CMSMasterStatusId;
                            //cms.Sequence = modelItem.Sequence ?? cms.Sequence;
                            //cms.CMSMasterURLKey = modelItem.URLKey != default(string) ? modelItem.URLKey : cms.CMSMasterURLKey;
                            //cms.Visibility = modelItem.Visibility ?? cms.Visibility;
                            //cms.UpdateBy = modelItem.CreateBy ?? cms.UpdateBy;
                            //cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = modelItem.CreateIP != default(string) ? modelItem.CreateIP : cms.UpdateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {


                                dbcxtransaction.Commit();

                                result = cms.CMSMasterId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();
                        return result;
                    }
                    //}
                    return result;
                }//end db transaction
            }

        }

        /// <summary>
        /// param as 
        /// </summary>
        public void EditCMSMainCategory() { }

        public void EditBrandInShop() { }


        public int EditCMSGroup(CMSGroupRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cms = db.CMSGroups.Where(c => c.CMSGroupId == Model.CMSGroupId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSGroupNameEN  = Model.CMSGroupNameEN;
                            cms.CMSGroupNameTH  = Model.CMSGroupNameTH;
                            cms.Status          = Model.Status;
                            //cms.Sequence        = Model.Sequence;
                            cms.Visibility      = Model.Visibility;
                            cms.UpdateBy        = Model.CreateBy;
                            cms.UpdateOn        = DateTime.Now;
                            cms.UpdateIP        = Model.CreateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = cms.CMSGroupId;
                            }
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        dbcxtransaction.Rollback();

                        return result;
                    }
                }
            }
        }

        #endregion

        #region new CMS CATE/COL/GROUP



        #endregion
    }
}