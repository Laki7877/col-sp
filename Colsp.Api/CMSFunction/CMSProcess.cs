﻿using System;
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
        /// <summary>
        /// param as CMSId,list of CMSCollection
        /// </summary>
        public bool CreateCMSCollectionItem(CMSCollectionItemRequest Model)
        {
            bool result = false;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        CM cms = new CM();
                        cms.CMSNameEN = Model.CMSNameEN;
                        cms.CMSNameTH = Model.CMSNameTH;
                        cms.CMSSortId = Model.CMSSortId;
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
                        cms.Visibility = Model.Visibility;
                        cms.CreateBy = Model.By;
                        cms.Createdate = DateTime.Now;
                        cms.CreateIP = Model.IP;
                        db.CMS.Add(cms);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            //History Log
                            CMSHistoryLogClass log = new CMSHistoryLogClass();
                            log.LogCreateCMS(cms.CMSId, "CMS", (bool)cms.Status, "Create", (int)cms.CreateBy, cms.CreateIP);
                            foreach (var item in Model.CollectionItemList)
                            {
                                CMSCollectionItem cItem = new CMSCollectionItem();
                                cItem.CMSId = cms.CMSId;//When saved has id.
                                cItem.PId = item.PId;
                                cItem.ProductBoxBadge = item.ProductBoxBadge;
                                cItem.Sequence = item.Sequence;
                                cItem.Status = item.Status;
                                cItem.CreateBy = Model.By;
                                cItem.Createdate = DateTime.Now;
                                cItem.CreateIP = Model.IP;
                                db.CMSCollectionItems.Add(cItem);
                                db.SaveChanges();
                                //History Log
                                CMSHistoryLogClass logCollection = new CMSHistoryLogClass();
                                log.LogCreateCMS(cItem.CMSCollectionItemId, "CMSCollectionItem", (bool)cItem.Status, "Create", (int)cItem.CreateBy, cItem.CreateIP);
                            }
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

        /// <summary>
        /// param as 
        /// </summary>
        public void CreateCMSMainCategory() { }

        public void CreateBrandInShop() { }

        #endregion

        #region Edit CMS

        public void Edit() { }

        /// <summary>
        /// param as cms class
        /// </summary>
        public void EditCMS() { }

        /// <summary>
        /// param as CMSId,list of CMSCollection
        /// </summary>
        public void EditCMSCollectionItem() { }

        /// <summary>
        /// param as 
        /// </summary>
        public void EditCMSMainCategory() { }

        public void EditBrandInShop() { }


        #endregion
    }
}