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
                        cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                        cms.CreateBy = Model.CreateBy;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.CreateIP;
                        db.CMSMasters.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            ////History Log
                            CMSHistoryLogClass log = new CMSHistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
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
                        cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
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
                                CMSCollectionItem cItem = new CMSCollectionItem();
                                cItem.CMSId = cms.CMSId;//When saved has id.
                                cItem.PId = item.PId;
                                cItem.ProductBoxBadge = item.ProductBoxBadge;
                                cItem.Sequence = item.Sequence;
                                cItem.Status = item.Status;
                                cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId;
                                cItem.CreateBy = Model.CreateBy;
                                cItem.Createdate = DateTime.Now;
                                cItem.CreateIP = Model.CreateIP;
                                db.CMSCollectionItems.Add(cItem);
                                db.SaveChanges();
                            }
                            result = cms.CMSId;
                            dbcxtransaction.Commit();
                            //History Log
                            CMSHistoryLogClass log = new CMSHistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMSMaster", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
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
                                CMSHistoryLogClass log = new CMSHistoryLogClass();
                                log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
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
            result.ShopId = model.ShopId;
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
        public bool CreateCMSMainCategory(CMSMainCategoryRequest Model)
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
                        cms.Status = Model.Status;
                        cms.CMSStatusFlowId = Model.CMSStatusId;
                        cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
                        cms.Sequence = Model.Sequence;
                        cms.URLKey = Model.URLKey;
                        cms.Visibility = Model.Visibility;
                        cms.CreateBy = Model.By;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.IP;
                        db.CMSMasters.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {

                            CMSMainCategory cItem = new CMSMainCategory();
                            cItem.CMSId = cms.CMSId;//When saved has id.
                            cItem.CategoryId = Model.CategoryId;
                            cItem.Status = Model.Status;
                            cItem.CreateBy = Model.By;
                            cItem.Createdate = DateTime.Now;
                            cItem.CreateIP = Model.IP;
                            db.CMSMainCategories.Add(cItem);
                            db.SaveChanges();
                            dbcxtransaction.Commit();
                            //History Log
                            CMSHistoryLogClass log = new CMSHistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);

                            CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
                            log.LogCreateCMS(cItem.CMSMainCategoryId, "CMSCollectionItem", (bool)cItem.Status, "Create", (int)cItem.CreateBy, cItem.CreateIP);

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
                        cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
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


                            CMSBrandInShop bItem = new CMSBrandInShop();
                            bItem.CMSId = cms.CMSId;//When saved has id.
                            bItem.BrandId = Model.BrandId;
                            bItem.BrandNameEN = Model.BrandNameEN;
                            bItem.BrandNameTH = Model.BrandNameTH;
                            bItem.LongDescriptionEn = Model.BrandLongDescriptionEn;
                            bItem.LongDescriptionTh = Model.BrandLongDescriptionTh;
                            bItem.MetaDescription = Model.MetaDescription;
                            bItem.MetaKey = Model.MetaKey;
                            bItem.MetaTitle = Model.MetaTitle;
                            bItem.Path = Model.Path;
                            bItem.PicUrl = Model.PicUrl;
                            bItem.ShortDescriptionEn = Model.ShortDescriptionEN;
                            bItem.ShortDescriptionTh = Model.ShortDescriptionTH;
                            bItem.UrlKey = Model.UrlKey;
                            bItem.Status = Model.Status;
                            bItem.CreateBy = Model.By;
                            bItem.Createdate = DateTime.Now;
                            bItem.CreateIP = Model.IP;
                            db.CMSBrandInShops.Add(bItem);
                            db.SaveChanges();
                            dbcxtransaction.Commit();
                            CMSHistoryLogClass log = new CMSHistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
                            log.LogCreateCMS(bItem.CMSBrandInShopId, "CMSBrandInShop", (bool)bItem.Status, "Create", (int)bItem.CreateBy, bItem.CreateIP);

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
                            cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
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
                                CMSHistoryLogClass log = new CMSHistoryLogClass();
                                log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
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
                            cms.CMSCollectionGroupId = Model.CMSCollectionGroupId;
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
                                    var cItem = db.CMSCollectionItems.Where(c => c.CMSCollectionItemId == item.CMSCollectionItemId).FirstOrDefault();
                                    if (cItem != null)
                                    {
                                        cItem.CMSId = cms.CMSId;//When saved has id.
                                        cItem.PId = item.PId;
                                        cItem.ProductBoxBadge = item.ProductBoxBadge;
                                        cItem.Sequence = item.Sequence;
                                        cItem.Status = item.Status;
                                        cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId;
                                        cItem.UpdateBy = Model.CreateBy;
                                        cItem.UpdateDate = DateTime.Now;
                                        cItem.UpdateIP = Model.CreateIP;
                                        db.Entry(cItem).State = EntityState.Modified;
                                        db.SaveChanges();
                                        dbcxtransaction.Commit();
                                        //History Log
                                        CMSHistoryLogClass log = new CMSHistoryLogClass();
                                        log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                        CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
                                        log.LogCreateCMS(cItem.CMSCollectionItemId, "CMSCollectionItem", (bool)cItem.Status, "Update", (int)cItem.CreateBy, cItem.CreateIP);
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
                            cms.CMSNameEN = Model.CMSNameEN != default(string) ? Model.CMSNameEN : cms.CMSNameEN ;
                            cms.CMSNameTH = Model.CMSNameTH != default(string) ? Model.CMSNameTH : cms.CMSNameTH;
                            cms.CMSFilterId = Model.CMSSortId ?? cms.CMSFilterId;
                            cms.CMSTypeId = Model.CMSTypeId ?? cms.CMSTypeId ;
                            cms.EffectiveDate = Model.EffectiveDate ?? cms.EffectiveDate ;
                            cms.EffectiveTime = Model.EffectiveTime ?? cms.EffectiveTime ;
                            cms.ExpiryDate = Model.ExpiryDate ?? cms.ExpiryDate ;
                            cms.ExpiryTime = Model.ExpiryTime ?? cms.ExpiryTime ;
                            cms.LongDescriptionEN = Model.LongDescriptionEN != default(string) ? Model.LongDescriptionEN : cms.LongDescriptionEN; 
                            cms.LongDescriptionTH = Model.LongDescriptionTH != default(string) ? Model.LongDescriptionTH : cms.LongDescriptionTH ; 
                            cms.ShopId = Model.ShopId ?? cms.ShopId;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN != default(string) ? Model.ShortDescriptionEN : cms.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH != default(string) ? Model.ShortDescriptionTH : cms.ShortDescriptionTH; 
                            cms.CMSCollectionGroupId = Model.CMSCollectionGroupId ?? cms.CMSCollectionGroupId ;
                            cms.Status = Model.Status ?? cms.Status;
                            cms.CMSStatusFlowId = Model.CMSStatusFlowId ?? cms.CMSStatusFlowId;
                            cms.Sequence = Model.Sequence ?? cms.Sequence;
                            cms.URLKey = Model.URLKey != default(string) ? Model.URLKey : cms.URLKey;
                            cms.Visibility = Model.Visibility ?? cms.Visibility;
                            cms.UpdateBy = Model.CreateBy ?? cms.UpdateBy;
                            cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP != default(string) ? Model.CreateIP : cms.UpdateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                //History Log
                                CMSHistoryLogClass log = new CMSHistoryLogClass();
                                log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
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
        public int UpdateCMSCollectionItem(CMSCollectionItemRequest Model)
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
                            cms.CMSNameEN = Model.CMSNameEN != default(string) ? Model.CMSNameEN : cms.CMSNameEN;
                            cms.CMSNameTH = Model.CMSNameTH != default(string) ? Model.CMSNameTH : cms.CMSNameTH;
                            cms.CMSFilterId = Model.CMSSortId ?? cms.CMSFilterId;
                            cms.CMSTypeId = Model.CMSTypeId ?? cms.CMSTypeId;
                            cms.EffectiveDate = Model.EffectiveDate ?? cms.EffectiveDate;
                            cms.EffectiveTime = Model.EffectiveTime ?? cms.EffectiveTime;
                            cms.ExpiryDate = Model.ExpiryDate ?? cms.ExpiryDate;
                            cms.ExpiryTime = Model.ExpiryTime ?? cms.ExpiryTime;
                            cms.LongDescriptionEN = Model.LongDescriptionEN != default(string) ? Model.LongDescriptionEN : cms.LongDescriptionEN;
                            cms.LongDescriptionTH = Model.LongDescriptionTH != default(string) ? Model.LongDescriptionTH : cms.LongDescriptionTH;
                            cms.ShopId = Model.ShopId ?? cms.ShopId;
                            cms.ShortDescriptionEN = Model.ShortDescriptionEN != default(string) ? Model.ShortDescriptionEN : cms.ShortDescriptionEN;
                            cms.ShortDescriptionTH = Model.ShortDescriptionTH != default(string) ? Model.ShortDescriptionTH : cms.ShortDescriptionTH;
                            cms.CMSCollectionGroupId = Model.CMSCollectionGroupId ?? cms.CMSCollectionGroupId;
                            cms.Status = Model.Status ?? cms.Status;
                            cms.CMSStatusFlowId = Model.CMSStatusFlowId ?? cms.CMSStatusFlowId;
                            cms.Sequence = Model.Sequence ?? cms.Sequence;
                            cms.URLKey = Model.URLKey != default(string) ? Model.URLKey : cms.URLKey;
                            cms.Visibility = Model.Visibility ?? cms.Visibility;
                            cms.UpdateBy = Model.CreateBy ?? cms.UpdateBy;
                            cms.UpdateDate = DateTime.Now;
                            cms.UpdateIP = Model.CreateIP != default(string) ? Model.CreateIP : cms.UpdateIP;
                            db.Entry(cms).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {

                                foreach (var item in Model.CollectionItemList)
                                {
                                    var cItem = db.CMSCollectionItems.Where(c => c.CMSCollectionItemId == item.CMSCollectionItemId).FirstOrDefault();
                                    if (cItem != null)
                                    {
                                        cItem.CMSId = cms.CMSId != default(int) ? cms.CMSId   : cItem.CMSId;//When saved has id.
                                        cItem.PId = item.PId != default(string) ? item.PId : cItem.PId; 
                                        cItem.ProductBoxBadge = item.ProductBoxBadge != default(string) ? item.ProductBoxBadge : cItem.ProductBoxBadge;
                                        cItem.Sequence = item.Sequence ?? cItem.Sequence;
                                        cItem.Status = item.Status ?? cItem.Status;
                                        cItem.CMSCollectionItemGroupId = item.CMSCollectionItemGroupId ?? cItem.CMSCollectionItemGroupId;
                                        cItem.UpdateBy = Model.CreateBy ?? cItem.UpdateBy;
                                        cItem.UpdateDate = DateTime.Now;
                                        cItem.UpdateIP = Model.CreateIP != default(string) ? Model.CreateIP : cItem.CreateIP;
                                        db.Entry(cItem).State = EntityState.Modified;
                                        db.SaveChanges();
                                        dbcxtransaction.Commit();
                                        //History Log
                                        CMSHistoryLogClass log = new CMSHistoryLogClass();
                                        log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Update", (int)cms.UpdateBy, cms.UpdateIP);
                                        CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
                                        log.LogCreateCMS(cItem.CMSCollectionItemId, "CMSCollectionItem", (bool)cItem.Status, "Update", (int)cItem.CreateBy, cItem.CreateIP);
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

        /// <summary>
        /// param as 
        /// </summary>
        public void EditCMSMainCategory() { }

        public void EditBrandInShop() { }


        #endregion
    }
}