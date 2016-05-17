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
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Colsp.Api.CMSFunction
{
    public class CMSHistoryLogClass : ApiController
    {
        #region CreateLog
        public void LogCreateCMS(int Id, string Tablename, bool? Status, string Transaction, int UserId, string IP)
        {
            var JsonText = "";
            switch (Tablename.ToUpper())
            {
                case "CMSMASTER":

                     JsonText = this.GetJsonMasterCMS(Id);
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, (bool?)Status, Transaction, UserId, IP);

                    break;
                case "CMSCOLLECTIONITEM":
                    JsonText = this.GetJsonCollectionItem(Id);
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, (bool?)Status, Transaction, UserId, IP);
                    break;
                //case "CMSMainCategory":
                //    JsonText = this.GetJsonMainCategory(Id);
                //    this.SaveCMSHistoryLog(Id, Tablename, JsonText, (bool?)Status, Transaction, UserId, IP);
                //    break;
                //case "CMSBrandInShop":
                //    JsonText = this.GetJsonBrandInShop(Id);
                //    this.SaveCMSHistoryLog(Id, Tablename, JsonText, (bool?)Status, Transaction, UserId, IP);
                //    break;
                case "CMSBY1GET1ITEM":
                    JsonText = this.GetJsonCMSBy1Get1Item(Id);
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, Status, Transaction, UserId, IP);
                    break;
                default:
                    break;

            }
        }
        #endregion

        #region SaveLog
        private bool SaveCMSHistoryLog(int Id, string Tablename, string JsonText, bool? Status, string Transaction, int UserId, string IP)

        {
            bool result = false;

            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    //CMSHistoryLog Cmslog = new CMSHistoryLog();
                    Cmslog.ChangeId = Id;
                    Cmslog.CMSTableLog = Tablename;
                    Cmslog.DetailLog = JsonText;
                    Cmslog.Status = Status;
                    Cmslog.TransactionLog = Transaction;
                    Cmslog.CreateBy = UserId;
                    Cmslog.Createdate = DateTime.Now;
                    Cmslog.CreateIP = IP;
                    //db.CMSHistoryLogs.Add(Cmslog);
                    if (db.SaveChanges() > 0)
                        result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result;
            }
        }
        #endregion
        
        #region Get Json Log

        private string GetJsonCollectionItem(int CollectionId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSCollectionItem = from n in db.CMSCategoryProductItems
                                        where n.CMSId == CollectionId
                                        select n;
                string output = new JavaScriptSerializer().Serialize(CMSCollectionItem);
                return output;

            }
        }

        private string GetJsonMasterCMS(int CMSId)
        {
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    var CMSModel = from n in db.CMSMasters
                                   where n.CMSId == CMSId
                                   select n;
                    string output = new JavaScriptSerializer().Serialize(CMSModel);
                    return output;

                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        //private string GetJsonMainCategory(int MainId)
        //{
        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        var CMSMainCat = db.CMSMainCategories.Where(c => c.CMSId == MainId).ToList();
        //        string output = new JavaScriptSerializer().Serialize(CMSMainCat);
        //        return output;
        //    }
        //}

        //private string GetJsonBrandInShop(int BrandId)
        //{
        //    using (ColspEntities db = new ColspEntities())
        //    {
        //        var CMSBrandInShop = db.CMSBrandInShops.Where(c => c.CMSId == BrandId).ToList();
        //        string output = new JavaScriptSerializer().Serialize(CMSBrandInShop);
        //        return output;


        //    }
        //}

        private string GetJsonCMSBy1Get1Item(int id)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSBy1Get1Item = from n in db.PromotionBuy1Get1Item
                                     where n.PromotionBuy1Get1ItemId == id
                                     select n;
                string output = new JavaScriptSerializer().Serialize(CMSBy1Get1Item);
                return output;

            }
        }
        #endregion

    }
}