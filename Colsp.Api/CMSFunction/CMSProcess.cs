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

        public int CreateCMSStaticPage(CMSCollectionItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSMaster cms = new CMSMaster();
                        cms.CMSNameEN = Model.CMSNameEN;
                        cms.CMSNameTH = Model.CMSNameTH;
                        cms.CMSFilterId = Model.CMSSortId;
                        cms.CMSTypeId = Model.CMSTypeId;
                        cms.EffectiveDate = Model.EffectiveDate;
                        cms.EffectiveTime = Model.EffectiveTime;
                        cms.ExpiryDate = Model.ExpiryDate;
                        cms.ExpiryTime = Model.ExpiryTime;
                        cms.LongDescriptionEN = Model.LongDescriptionEN;
                        cms.LongDescriptionTH = Model.LongDescriptionTH;
                        cms.ShopId = Model.ShopId;
                        cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                        cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                        cms.Status = Model.Status;
                        cms.CMSStatusFlowId = Model.CMSStatusFlowId;
                        cms.Sequence = Model.Sequence;
                        cms.URLKey = Model.URLKey;
                        cms.Visibility = Model.Visibility;
                        //cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                        cms.CreateBy = Model.CreateBy;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        db.CMSMasters.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            ////History Log
                            HistoryLogClass log = new HistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            result = cms.CMSId;
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
        /// param as CMSId,list of CMSCollection
        /// </summary>
        public int CreateCMSCollectionItem(CMSCollectionItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSMaster cms = new CMSMaster();
                        cms.CMSNameEN = Model.CMSNameEN;
                        cms.CMSNameTH = Model.CMSNameTH;
                        cms.CMSFilterId = Model.CMSSortId;
                        cms.CMSTypeId = Model.CMSTypeId;
                        cms.EffectiveDate = Model.EffectiveDate;
                        cms.EffectiveTime = Model.EffectiveTime;
                        cms.ExpiryDate = Model.ExpiryDate;
                        cms.ExpiryTime = Model.ExpiryTime;
                        cms.LongDescriptionEN = Model.LongDescriptionEN;
                        cms.LongDescriptionTH = Model.LongDescriptionTH;
                        cms.ShopId = Model.ShopId;
                        cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                        cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                        cms.Status = Model.Status;
                        cms.CMSStatusFlowId = Model.CMSStatusFlowId;
                        //cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                        cms.Sequence = Model.Sequence;
                        cms.URLKey = Model.URLKey;
                        cms.Visibility = Model.Visibility;
                        cms.CreateBy = Model.CreateBy;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        db.CMSMasters.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {

                            foreach (var item in Model.CollectionItemList)
                            {
                                CMSCategoryProductItem cItem = new CMSCategoryProductItem();
                                cItem.CMSId = cms.CMSId;//When saved has id.
                                cItem.PId = item.PId;
                                cItem.ProductBoxBadge = item.ProductBoxBadge;
                                cItem.Sequence = item.Sequence;
                                cItem.Status = item.Status;
                                //cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId;
                                cItem.CreateBy = Model.CreateBy;
                                cItem.Createdate = DateTime.Now;
                                cItem.CreateIP = Model.CreateIP;
                                db.CMSCategoryProductItems.Add(cItem);
                                db.SaveChanges();
                            }
                            result = cms.CMSId;
                            dbcxtransaction.Commit();
                            //History Log
                            HistoryLogClass log = new HistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMSMaster", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            HistoryLogClass logCollection = new HistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMSCollectionItem", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
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

        public CMSShopRequest CMSUpdateStatus(UpdateCMSStatusRequest model)
        {
            CMSShopRequest result = new CMSShopRequest();
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    foreach (var item in model.CMSList)
                    {
                        try
                        {
                            var cms = db.CMSMasters.Where(c => c.CMSId == item.CMSId).FirstOrDefault();
                            if (cms != null)
                            {
                                cms.CMSStatusFlowId = item.CMSStatusId;
                                cms.Status = item.Status;
                                cms.Visibility = item.CMSVisibility;
                                cms.UpdateBy = model.UserId;
                                cms.UpdateDate = DateTime.Now;
                                cms.UpdateIP = model.CreateIP;
                                db.Entry(cms).State = EntityState.Modified;
                                db.SaveChanges();
                                dbcxtransaction.Commit();
                                //History Log
                                HistoryLogClass log = new HistoryLogClass();
                                log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
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
            //result.ShopId = model.ShopId;
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
        public bool CreateCMSMainCategory(CMSGroupRequest Model)
        {
            bool result = false;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSGroup cms = new CMSGroup();
                        cms.CMSGroupNameEN = Model.CMSGroupNameEN;
                        cms.CMSGroupNameTH = Model.CMSGroupNameTH;
                        cms.ShopId = Model.ShopId;
                        cms.BannerLocation = Model.BannerConntent;
                        cms.BannerConntent = Model.BannerConntent;
                        cms.Status = Model.Status;
                        cms.Sequence = Model.Sequence;
                        cms.Visibility = Model.Visibility;
                        cms.CreateBy = Model.CreateBy;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        db.CMSGroups.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {

                            //CMSMainCategory cItem = new CMSMainCategory();
                            //cItem.CMSId = cms.CMSId;//When saved has id.
                            //cItem.CategoryId = Model.CategoryId;
                            //cItem.Status = Model.Status;
                            //cItem.CreateBy = Model.By;
                            //cItem.Createdate = DateTime.Now;
                            //cItem.CreateIP = Model.IP;
                            //db.CMSMainCategories.Add(cItem);
                            //db.SaveChanges();
                            //dbcxtransaction.Commit();
                            //History Log
                            //HistoryLogClass log = new HistoryLogClass();
                            //log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);

                            //HistoryLogClass logCollection = new HistoryLogClass();
                          // log.LogCreateCMS(cItem.CMSMainCategoryId, "CMSCollectionItem", cItem.Status, "Create", (int)cItem.CreateBy, cItem.CreateIP);

                            result = true;

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

        public bool CreateBrandInShop(CMSBrandInShopRequest Model)
        {

            bool result = false;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CMSMaster cms = new CMSMaster();
                        cms.CMSNameEN = Model.CMSNameEN;
                        cms.CMSNameTH = Model.CMSNameTH;
                        cms.CMSFilterId = Model.CMSSortId;
                        cms.CMSTypeId = Model.CMSTypeId;
                        cms.EffectiveDate = Model.EffectiveDate;
                        cms.EffectiveTime = Model.EffectiveTime;
                        cms.ExpiryDate = Model.ExpiryDate;
                        cms.ExpiryTime = Model.ExpiryTime;
                        cms.LongDescriptionEN = Model.LongDescriptionEN;
                        cms.LongDescriptionTH = Model.LongDescriptionTH;
                        cms.ShopId = Model.ShopId;
                        cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                        cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                        //cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                        cms.Status = Model.Status;
                        cms.CMSStatusFlowId = Model.CMSStatusId;
                        cms.Sequence = Model.Sequence;
                        cms.URLKey = Model.URLKey;
                        cms.Visibility = Model.Visibility;
                        cms.CreateBy = Model.By;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.IP;
                        db.CMSMasters.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {


                            //CMSBrandInShop bItem = new CMSBrandInShop();
                            //bItem.CMSId = cms.CMSId;//When saved has id.
                            //bItem.BrandId = Model.BrandId;
                            //bItem.BrandNameEN = Model.BrandNameEN;
                            //bItem.BrandNameTH = Model.BrandNameTH;
                            //bItem.LongDescriptionEn = Model.BrandLongDescriptionEn;
                            //bItem.LongDescriptionTh = Model.BrandLongDescriptionTh;
                            //bItem.MetaDescription = Model.MetaDescription;
                            //bItem.MetaKey = Model.MetaKey;
                            //bItem.MetaTitle = Model.MetaTitle;
                            //bItem.Path = Model.Path;
                            //bItem.PicUrl = Model.PicUrl;
                            //bItem.ShortDescriptionEn = Model.ShortDescriptionEN;
                            //bItem.ShortDescriptionTh = Model.ShortDescriptionTH;
                            //bItem.UrlKey = Model.UrlKey;
                            //bItem.Status = Model.Status;
                            //bItem.CreateBy = Model.By;
                            //bItem.Createdate = DateTime.Now;
                            //bItem.CreateIP = Model.IP;
                            //db.CMSBrandInShops.Add(bItem);
                           // db.SaveChanges();
                            dbcxtransaction.Commit();
                            HistoryLogClass log = new HistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            HistoryLogClass logCollection = new HistoryLogClass();
                          //  log.LogCreateCMS(bItem.CMSBrandInShopId, "CMSBrandInShop", bItem.Status, "Create", (int)bItem.CreateBy, bItem.CreateIP);

                            result = true;

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
                        var cms = db.CMSMasters.Where(c => c.CMSId == Model.CMSId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSNameEN = Model.CMSNameEN;
                            cms.CMSNameTH = Model.CMSNameTH;
                            cms.CMSFilterId = Model.CMSSortId;
                            cms.CMSTypeId = Model.CMSTypeId;
                            cms.EffectiveDate = Model.EffectiveDate;
                            cms.EffectiveTime = Model.EffectiveTime;
                            cms.ExpiryDate = Model.ExpiryDate;
                            cms.ExpiryTime = Model.ExpiryTime;
                            cms.LongDescriptionEN = Model.LongDescriptionEN;
                            cms.LongDescriptionTH = Model.LongDescriptionTH;
                            cms.ShopId = Model.ShopId;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                            //cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                            cms.Status = Model.Status;
                            cms.CMSStatusFlowId = Model.CMSStatusFlowId;
                            cms.Sequence = Model.Sequence;
                            cms.URLKey = Model.URLKey;
                            cms.Visibility = Model.Visibility;
                            cms.UpdateBy = Model.CreateBy;
                            cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                //History Log
                                HistoryLogClass log = new HistoryLogClass();
                                log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                result = cms.CMSId;
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
                        var cms = db.CMSMasters.Where(c => c.CMSId == Model.CMSId).FirstOrDefault();
                        if (cms != null)
                        {
                            cms.CMSNameEN = Model.CMSNameEN;
                            cms.CMSNameTH = Model.CMSNameTH;
                            cms.CMSFilterId = Model.CMSSortId;
                            cms.CMSTypeId = Model.CMSTypeId;
                            cms.EffectiveDate = Model.EffectiveDate;
                            cms.EffectiveTime = Model.EffectiveTime;
                            cms.ExpiryDate = Model.ExpiryDate;
                            cms.ExpiryTime = Model.ExpiryTime;
                            cms.LongDescriptionEN = Model.LongDescriptionEN;
                            cms.LongDescriptionTH = Model.LongDescriptionTH;
                            cms.ShopId = Model.ShopId;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH;
                            cms.Status = Model.Status;
                            cms.CMSStatusFlowId = Model.CMSStatusFlowId;
                            cms.Sequence = Model.Sequence;
                            //cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                            cms.URLKey = Model.URLKey;
                            cms.Visibility = Model.Visibility;
                            cms.UpdateBy = Model.CreateBy;
                            cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {

                                foreach (var item in Model.CollectionItemList)
                                {
                                    var cItem = db.CMSCategoryProductItems.Where(c => c.CMSCollectionListItemId == item.CMSCollectionListItemId).FirstOrDefault();
                                    if (cItem != null)
                                    {
                                        cItem.CMSId = cms.CMSId;//When saved has id.
                                        cItem.PId = item.PId;
                                        cItem.ProductBoxBadge = item.ProductBoxBadge;
                                        cItem.Sequence = item.Sequence;
                                        cItem.Status = item.Status;
                                        //cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId;
                                        cItem.UpdateBy = Model.CreateBy;
                                        cItem.UpdateDate = DateTime.Now;
                                        cItem.UpdateIP = Model.CreateIP;
                                        db.Entry(cItem).State = EntityState.Modified;
                                        db.SaveChanges();
                                        dbcxtransaction.Commit();
                                        //History Log
                                        HistoryLogClass log = new HistoryLogClass();
                                        log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                        HistoryLogClass logCollection = new HistoryLogClass();
                                        log.LogCreateCMS(cItem.CMSCollectionListItemId, "CMSCollectionItem", cItem.Status, "Update", (int)cItem.CreateBy, cItem.CreateIP);
                                    }
                                }
                                result = cms.CMSId;
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
        public int UpdateCMSStaticPage(CMSCollectionItemRequest Model)
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
                            var cms = db.CMSMasters.Where(c => c.CMSId == modelItem.CMSId).FirstOrDefault();
                            if (cms != null)
                            {

                                cms.CMSNameEN = modelItem.CMSNameEN != default(string) ? modelItem.CMSNameEN : cms.CMSNameEN;
                                cms.CMSNameTH = modelItem.CMSNameTH != default(string) ? modelItem.CMSNameTH : cms.CMSNameTH;
                                cms.CMSFilterId = modelItem.CMSSortId ?? cms.CMSFilterId;
                                cms.CMSTypeId = modelItem.CMSTypeId ?? cms.CMSTypeId;
                                cms.EffectiveDate = modelItem.EffectiveDate ?? cms.EffectiveDate;
                                cms.EffectiveTime = modelItem.EffectiveTime ?? cms.EffectiveTime;
                                cms.ExpiryDate = modelItem.ExpiryDate ?? cms.ExpiryDate;
                                cms.ExpiryTime = modelItem.ExpiryTime ?? cms.ExpiryTime;
                                cms.LongDescriptionEN = modelItem.LongDescriptionEN != default(string) ? modelItem.LongDescriptionEN : cms.LongDescriptionEN;
                                cms.LongDescriptionTH = modelItem.LongDescriptionTH != default(string) ? modelItem.LongDescriptionTH : cms.LongDescriptionTH;
                                cms.ShopId = modelItem.ShopId ?? cms.ShopId;
                                cms.ShortDescriptionEN = modelItem.ShortDescriptionEN != default(string) ? modelItem.ShortDescriptionEN : cms.ShortDescriptionEN;
                                cms.ShortDescriptionTH = modelItem.ShortDescriptionTH != default(string) ? modelItem.ShortDescriptionTH : cms.ShortDescriptionTH;
                                //cms.CMSCollectionGroupId = modelItem.CMSCollectionGroupId ?? cms.CMSCollectionGroupId;
                                cms.Status = modelItem.Status ?? cms.Status;
                                cms.CMSStatusFlowId = modelItem.CMSStatusFlowId ?? cms.CMSStatusFlowId;
                                cms.Sequence = modelItem.Sequence ?? cms.Sequence;
                                cms.URLKey = modelItem.URLKey != default(string) ? modelItem.URLKey : cms.URLKey;
                                cms.Visibility = modelItem.Visibility ?? cms.Visibility;
                                cms.UpdateBy = modelItem.CreateBy ?? cms.UpdateBy;
                                cms.UpdateDate = DateTime.Now;
                                cms.UpdateIP = modelItem.CreateIP != default(string) ? modelItem.CreateIP : cms.UpdateIP;
                                db.Entry(cms).State = EntityState.Modified;
                                if (db.SaveChanges() > 0) //Saved return row save successfully.
                                {
                                    dbcxtransaction.Commit();
                                    //History Log
                                    HistoryLogClass log = new HistoryLogClass();
                                    log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                    result = cms.CMSId;
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
                            var cms = db.CMSMasters.Where(c => c.CMSId == modelItem.CMSId).FirstOrDefault();
                            if (cms != null)
                            {
                                cms.CMSNameEN = modelItem.CMSNameEN != default(string) ? modelItem.CMSNameEN : cms.CMSNameEN;
                                cms.CMSNameTH = modelItem.CMSNameTH != default(string) ? modelItem.CMSNameTH : cms.CMSNameTH;
                                cms.CMSFilterId = modelItem.CMSSortId ?? cms.CMSFilterId;
                                cms.CMSTypeId = modelItem.CMSTypeId ?? cms.CMSTypeId;
                                cms.EffectiveDate = modelItem.EffectiveDate ?? cms.EffectiveDate;
                                cms.EffectiveTime = modelItem.EffectiveTime ?? cms.EffectiveTime;
                                cms.ExpiryDate = modelItem.ExpiryDate ?? cms.ExpiryDate;
                                cms.ExpiryTime = modelItem.ExpiryTime ?? cms.ExpiryTime;
                                cms.LongDescriptionEN = modelItem.LongDescriptionEN != default(string) ? modelItem.LongDescriptionEN : cms.LongDescriptionEN;
                                cms.LongDescriptionTH = modelItem.LongDescriptionTH != default(string) ? modelItem.LongDescriptionTH : cms.LongDescriptionTH;
                                cms.ShopId = modelItem.ShopId ?? cms.ShopId;
                                cms.ShortDescriptionEN = modelItem.ShortDescriptionEN != default(string) ? modelItem.ShortDescriptionEN : cms.ShortDescriptionEN;
                                cms.ShortDescriptionTH = modelItem.ShortDescriptionTH != default(string) ? modelItem.ShortDescriptionTH : cms.ShortDescriptionTH;
                                //cms.CMSCollectionGroupId = modelItem.CMSCollectionGroupId ?? cms.CMSCollectionGroupId;
                                cms.Status = modelItem.Status ?? cms.Status;
                                cms.CMSStatusFlowId = modelItem.CMSStatusFlowId ?? cms.CMSStatusFlowId;
                                cms.Sequence = modelItem.Sequence ?? cms.Sequence;
                                cms.URLKey = modelItem.URLKey != default(string) ? modelItem.URLKey : cms.URLKey;
                                cms.Visibility = modelItem.Visibility ?? cms.Visibility;
                                cms.UpdateBy = modelItem.CreateBy ?? cms.UpdateBy;
                                cms.UpdateDate = DateTime.Now;
                                cms.UpdateIP = modelItem.CreateIP != default(string) ? modelItem.CreateIP : cms.UpdateIP;
                                db.Entry(cms).State = EntityState.Modified;
                                if (db.SaveChanges() > 0) //Saved return row save successfully.
                                {

                                    foreach (var item in modelItem.CollectionItemList)
                                    {
                                    var cItem = db.CMSCategoryProductItems.Where(c => c.CMSCollectionListItemId == item.CMSCollectionListItemId).FirstOrDefault();
                                        if (cItem != null)
                                        {
                                            cItem.CMSId = cms.CMSId != default(int) ? cms.CMSId : cItem.CMSId;//When saved has id.
                                            cItem.PId = item.PId != default(string) ? item.PId : cItem.PId;
                                            cItem.ProductBoxBadge = item.ProductBoxBadge != default(string) ? item.ProductBoxBadge : cItem.ProductBoxBadge;
                                            cItem.Sequence = item.Sequence ?? cItem.Sequence;
                                            cItem.Status = item.Status ?? cItem.Status;
                                            //cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId ?? cItem.CMSCollectionItemGroupId;
                                            cItem.UpdateBy = modelItem.CreateBy ?? cItem.UpdateBy;
                                            cItem.UpdateDate = DateTime.Now;
                                            cItem.UpdateIP = modelItem.CreateIP != default(string) ? modelItem.CreateIP : cItem.CreateIP;
                                            db.Entry(cItem).State = EntityState.Modified;
                                            db.SaveChanges();
                                            dbcxtransaction.Commit();
                                            //History Log
                                            HistoryLogClass log = new HistoryLogClass();
                                            log.LogCreateCMS(cms.CMSId, "CMS", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                            HistoryLogClass logCollection = new HistoryLogClass();
                                            log.LogCreateCMS(cItem.CMSCollectionListItemId, "CMSCollectionItem", cItem.Status, "Update", (int)cItem.CreateBy, cItem.CreateIP);
                                        }
                                    }
                                    result = cms.CMSId;
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


        #endregion

        #region new CMS CATE/COL/GROUP
        //public int CreateCMSCollectionCategory(CMSCollectionCategoryRequest Model)
        //{
        //    int result = 0;
        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        using (var dbcxtransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                CMSCollectionCategory cms = new CMSCollectionCategory();
        //              //cms.CMSCollectionCategoryId       = Model.CMSCollectionCategoryId     ;
        //              cms.CMSCollectionCategoryNameEN   = Model.CMSCollectionCategoryNameEN ;
        //              cms.CMSCollectionCategoryNameTH   = Model.CMSCollectionCategoryNameTH ;
        //              cms.Status                        = Model.Status                      ;
        //              cms.Visibility                    = Model.Visibility                  ;
        //              cms.CreateBy                      = Model.CreateBy                    ;
        //              cms.Createdate                    = DateTime.Now                  ;
        //              cms.UpdateBy                      = Model.UpdateBy                    ;
        //              cms.UpdateDate                    = DateTime.Now                  ;
        //              cms.CreateIP                      = Model.CreateIP                    ;
        //              cms.UpdateIP                      = Model.UpdateIP                    ;
        //              cms.EffectiveDate                 = Model.EffectiveDate               ;
        //              cms.EffectiveTime                 = Model.EffectiveTime               ;
        //              cms.ExpiryDate                    = Model.ExpiryDate                  ;
        //              cms.ExpiryTime                    = Model.ExpiryTime                  ;
        //              cms.CMSStatusFlowId               = Model.CMSStatusFlowId             ;
        //              cms.Sequence                      = Model.Sequence                    ;

        //                db.CMSCollectionCategories.Add(cms);
        //                if (db.SaveChanges() > 0) //Saved return row save successfully.
        //                {
        //                    foreach (var item in Model.CMSRelProductCategory)
        //                    {
        //                        CMSRelProductCategory cmsRel = new CMSRelProductCategory();
        //                        //cmsRel.CMSRelProductCategoryId  = item.CMSRelProductCategoryId ;
        //                        cmsRel.ProductId                = item.ProductId               ;
        //                        cmsRel.Status                   = true ;                // item.Status                  ;
        //                        cmsRel.Sequence                 = item.Sequence                ;
        //                        cmsRel.Visibility               = item.Visibility              ;
        //                        cmsRel.CreateBy                 = Model.CreateBy    ;//item.CreateBy                ;
        //                        cmsRel.Createdate               = Model.Createdate  ;//item.Createdate              ;
        //                        cmsRel.UpdateBy                 = Model.UpdateBy    ;//item.UpdateBy                ;
        //                        cmsRel.UpdateDate               = Model.UpdateDate  ;//item.UpdateDate              ;
        //                        cmsRel.CreateIP                 = Model.CreateIP    ;//item.CreateIP                ;
        //                        cmsRel.UpdateIP                 = Model.UpdateIP    ;//item.UpdateIP                ;
        //                        cmsRel.EffectiveDate            = null              ;//item.EffectiveDate           ;
        //                        cmsRel.EffectiveTime            = null              ;//item.EffectiveTime           ;
        //                        cmsRel.ExpiryDate               = null              ;//item.ExpiryDate              ;
        //                        cmsRel.ExpiryTime               = null              ;//item.ExpiryTime              ;
        //                        cmsRel.CMSCollectionCategoryId = cms.CMSCollectionCategoryId    ;
        //                        db.CMSRelProductCategories.Add(cmsRel);
        //                        db.SaveChanges();
        //                    }
        //                    result = cms.CMSCollectionCategoryId;
        //                    dbcxtransaction.Commit();
        //                    //History Log
        //                    HistoryLogClass log = new HistoryLogClass();
        //                    log.LogCreateCMS(cms.CMSCollectionCategoryId, "CMSMainCategory", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
        //                }
        //                return result;
        //            }
        //            catch (Exception ex)
        //            {
        //                dbcxtransaction.Rollback();
        //                return result;
        //            }
        //        }
        //    }

        //}

        //public int EditCMSCollectionCategory(CMSCollectionCategoryRequest Model)
        //{
        //    var modelItem = Model;
        //    int result = 0;
        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        using (var dbcxtransaction = db.Database.BeginTransaction())
        //        {
        //            //foreach (var modelItem in Model) {
        //            try
        //            {
        //                var cms = db.CMSCollectionCategories.Where(c => c.CMSCollectionCategoryId == modelItem.CMSCollectionCategoryId).FirstOrDefault();
        //                if (cms != null)
        //                {                                                        
        //                    cms.CMSCollectionCategoryNameEN = Model.CMSCollectionCategoryNameEN != default(string) ? Model.CMSCollectionCategoryNameEN : null ;
        //                    cms.CMSCollectionCategoryNameTH = Model.CMSCollectionCategoryNameTH != default(string) ? Model.CMSCollectionCategoryNameTH : null;
        //                    cms.Status                      = Model.Status ?? null;
        //                    cms.Visibility                  = Model.Visibility ?? null;
        //                    cms.CreateBy                    = Model.CreateBy ?? null;
        //                    cms.Createdate                  = Model.Createdate ?? null;
        //                    cms.UpdateBy                    = Model.UpdateBy ?? null;
        //                    cms.UpdateDate                  = Model.UpdateDate ?? null;
        //                    cms.CreateIP                    = Model.CreateIP != default(string) ? Model.CreateIP : null;
        //                    cms.UpdateIP                    = Model.UpdateIP != default(string) ? Model.UpdateIP : null;
        //                    cms.EffectiveDate               = Model.EffectiveDate ?? null;
        //                    cms.EffectiveTime               = Model.EffectiveTime ?? null;
        //                    cms.ExpiryDate                  = Model.ExpiryDate ?? null;
        //                    cms.ExpiryTime                  = Model.ExpiryTime ?? null;
        //                    cms.CMSStatusFlowId             = Model.CMSStatusFlowId ?? null;
        //                    cms.Sequence                    = Model.Sequence  ?? null                  ;

        //                    db.Entry(cms).State = EntityState.Modified;
        //                    if (db.SaveChanges() > 0) //Saved return row save successfully.
        //                    {
        //                        //clrear all status like del all
        //                        foreach (var item in Model.CMSRelProductCategory)
        //                        {
        //                            var cItem = db.CMSRelProductCategories.Where(c => c.CMSRelProductCategoryId == item.CMSRelProductCategoryId).FirstOrDefault();
        //                            if (cItem != null)
        //                            {
        //                                cItem.Status = false;
        //                            }
        //                            db.Entry(cItem).State = EntityState.Modified;
        //                            db.SaveChanges();
        //                        };

        //                        foreach (var item in Model.CMSRelProductCategory)
        //                        {
        //                            var cItem = db.CMSRelProductCategories.Where(c => c.CMSRelProductCategoryId == item.CMSRelProductCategoryId).FirstOrDefault();
        //                            if (cItem != null)
        //                            {                                      
        //                                cItem.Status                        = true ;
        //                                cItem.Visibility                    = item.Visibility ?? false ;
        //                                cItem.CreateBy                      = item.CreateBy ?? null ;
        //                                cItem.Createdate                    = item.Createdate ?? null ;
        //                                cItem.UpdateBy                      = item.UpdateBy ?? null ;
        //                                cItem.UpdateDate                    = item.UpdateDate ?? null ;
        //                                cItem.CreateIP                      = item.CreateIP != default(string) ? cms.CreateIP : null ;
        //                                cItem.UpdateIP                      = item.UpdateIP != default(string) ? cms.UpdateIP : null ;
        //                                cItem.EffectiveDate                 = item.EffectiveDate.HasValue ? cms.EffectiveDate : null ;
        //                                cItem.EffectiveTime                 = item.EffectiveTime.HasValue ? cms.EffectiveTime : null ;
        //                                cItem.ExpiryDate                    = item.ExpiryDate.HasValue ? cms.ExpiryDate : null ;
        //                                cItem.ExpiryTime                    = item.ExpiryTime.HasValue ? cms.ExpiryTime : null ;
        //                                cItem.ProductId                     = item.ProductId ?? null ;
        //                                cItem.Sequence                      = item.Sequence ?? null;

        //                                db.Entry(cItem).State = EntityState.Modified;
        //                                db.SaveChanges();
                                        
        //                                //History Log
        //                                HistoryLogClass log = new HistoryLogClass();
        //                                log.LogCreateCMS(cms.CMSCollectionCategoryId, "CMSMainCategory", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);                                       
        //                            }
        //                        }
                                
        //                        result = cms.CMSCollectionCategoryId;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                dbcxtransaction.Rollback();
        //                return result;
        //            }
        //            //}
        //            dbcxtransaction.Commit();
        //            return result;
        //        }//end db transaction
        //    }

        //}

        //public int UpdateCMSCollectionCategory(CMSCollectionCategoryRequest Model)
        //{
        //    var modelItem = Model;
        //    int result = 0;
        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        using (var dbcxtransaction = db.Database.BeginTransaction())
        //        {
        //            //foreach (var modelItem in Model) {
        //            try
        //            {
        //                var cms = db.CMSCollectionCategories.Where(c => c.CMSCollectionCategoryId == modelItem.CMSCollectionCategoryId).FirstOrDefault();
        //                if (cms != null)
        //                {
        //                    cms.CMSCollectionCategoryNameEN = Model.CMSCollectionCategoryNameEN != default(string) ? Model.CMSCollectionCategoryNameEN : cms.CMSCollectionCategoryNameEN;
        //                    cms.CMSCollectionCategoryNameTH = Model.CMSCollectionCategoryNameTH != default(string) ? Model.CMSCollectionCategoryNameTH : cms.CMSCollectionCategoryNameTH;
        //                    cms.Status = Model.Status ?? cms.Status;
        //                    cms.Visibility = Model.Visibility ?? cms.Visibility;
        //                    cms.CreateBy = Model.CreateBy ?? cms.CreateBy;
        //                    cms.Createdate = Model.Createdate ?? cms.Createdate;
        //                    cms.UpdateBy = Model.UpdateBy ?? cms.UpdateBy;
        //                    cms.UpdateDate = Model.UpdateDate ?? cms.UpdateDate;
        //                    cms.CreateIP = Model.CreateIP != default(string) ? Model.CreateIP : cms.CreateIP;
        //                    cms.UpdateIP = Model.UpdateIP != default(string) ? Model.UpdateIP : cms.UpdateIP;
        //                    cms.EffectiveDate = Model.EffectiveDate ?? cms.EffectiveDate;
        //                    cms.EffectiveTime = Model.EffectiveTime ?? cms.EffectiveTime;
        //                    cms.ExpiryDate = Model.ExpiryDate ?? cms.ExpiryDate;
        //                    cms.ExpiryTime = Model.ExpiryTime ?? cms.ExpiryTime;
        //                    cms.CMSStatusFlowId = Model.CMSStatusFlowId ?? cms.CMSStatusFlowId;
        //                    cms.Sequence = Model.Sequence ?? cms.Sequence;

        //                    db.Entry(cms).State = EntityState.Modified;
        //                    if (db.SaveChanges() > 0) //Saved return row save successfully.
        //                    {
        //                        //clrear all status like del all
        //                        foreach (var item in Model.CMSRelProductCategory)
        //                        {
        //                            var cItem = db.CMSRelProductCategories.Where(c => c.CMSRelProductCategoryId == item.CMSRelProductCategoryId).FirstOrDefault();
        //                            if (cItem != null)
        //                            {
        //                                cItem.Status = false;
        //                            }
        //                            db.Entry(cItem).State = EntityState.Modified;
        //                            db.SaveChanges();
        //                        };

        //                        foreach (var item in Model.CMSRelProductCategory)
        //                        {
        //                            var cItem = db.CMSRelProductCategories.Where(c => c.CMSRelProductCategoryId == item.CMSRelProductCategoryId).FirstOrDefault();
        //                            if (cItem != null)
        //                            {
        //                                cItem.Status = true;
        //                                cItem.Visibility = item.Visibility ?? cItem.Visibility;
        //                                cItem.CreateBy = item.CreateBy ?? cItem.CreateBy;
        //                                cItem.Createdate = item.Createdate ?? cItem.Createdate;
        //                                cItem.UpdateBy = item.UpdateBy ?? cItem.UpdateBy;
        //                                cItem.UpdateDate = item.UpdateDate ?? cItem.UpdateDate;
        //                                cItem.CreateIP = item.CreateIP != default(string) ? cms.CreateIP : cItem.CreateIP;
        //                                cItem.UpdateIP = item.UpdateIP != default(string) ? cms.UpdateIP : cItem.UpdateIP;
        //                                cItem.EffectiveDate = item.EffectiveDate.HasValue ? cms.EffectiveDate : cItem.EffectiveDate;
        //                                cItem.EffectiveTime = item.EffectiveTime.HasValue ? cms.EffectiveTime : cItem.EffectiveTime;
        //                                cItem.ExpiryDate = item.ExpiryDate.HasValue ? cms.ExpiryDate : cItem.ExpiryDate;
        //                                cItem.ExpiryTime = item.ExpiryTime.HasValue ? cms.ExpiryTime : cItem.ExpiryTime;
        //                                cItem.ProductId = item.ProductId ?? cItem.ProductId;
        //                                cItem.Sequence = item.Sequence ?? cItem.Sequence;

        //                                db.Entry(cItem).State = EntityState.Modified;
        //                                db.SaveChanges();

        //                                //History Log
        //                                HistoryLogClass log = new HistoryLogClass();
        //                                log.LogCreateCMS(cms.CMSCollectionCategoryId, "CMSMainCategory", cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
        //                            }
        //                        }

        //                        result = cms.CMSCollectionCategoryId;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                dbcxtransaction.Rollback();
        //                return result;
        //            }
        //            //}
        //            dbcxtransaction.Commit();
        //            return result;
        //        }//end db transaction
        //    }

        //}
        #endregion
    }
}