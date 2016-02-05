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
    public class CMSHistoryLogClass
    {
        public void LogCreateCMS(int Id, string Tablename, bool Status, string Transaction, int UserId, string IP)
        {
            string JsonText = "";
            switch (Tablename.ToUpper())
            {
                case "CMS":
                    JsonText = this.GetJsonMasterCMS(Id).ToString();
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, Status, Transaction, UserId, IP);
                    break;
                case "CMSCollectionItem":
                    JsonText = this.GetJsonCollectionItem(Id).ToString();
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, Status, Transaction, UserId, IP);
                    break;
                case "CMSMainCategory":
                    JsonText = this.GetJsonMainCategory(Id).ToString();
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, Status, Transaction, UserId, IP);
                    break;
                case "CMSBrandInShop":
                    JsonText = this.GetJsonBrandInShop(Id).ToString();
                    this.SaveCMSHistoryLog(Id, Tablename, JsonText, Status, Transaction, UserId, IP);
                    break;
                default:
                    break;

            }
        }

        private bool SaveCMSHistoryLog(int Id, string Tablename, string JsonText, bool Status, string Transaction, int UserId, string IP)
        {
            bool result = false;
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    CMSHistoryLog Cmslog = new CMSHistoryLog();
                    Cmslog.ChangeId = Id;
                    Cmslog.CMSTableLog = Tablename;
                    Cmslog.DetailLog = JsonText;
                    Cmslog.Status = Status;
                    Cmslog.TransactionLog = Transaction;
                    Cmslog.CreateBy = UserId;
                    Cmslog.Createdate = DateTime.Now;
                    Cmslog.CreateIP = IP;
                    db.CMSHistoryLogs.Add(Cmslog);
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


        private JsonContent GetJsonCollectionItem(int CollectionId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSCollectionItem = db.CMSCollectionItems.Where(c => c.CMSId == CollectionId).ToList();

                return new JsonContent(new
                {
                    Data = CMSCollectionItem
                });
            }
        }

        private JsonContent GetJsonMasterCMS(int CMSId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSModel = db.CMS.Where(c => c.CMSId == CMSId).ToList();
                return new JsonContent(new
                {
                    Data = CMSModel
                });

            }
        }

        private JsonContent GetJsonMainCategory(int MainId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSMainCat = db.CMSMainCategories.Where(c => c.CMSId == MainId).ToList();
                return new JsonContent(new
                {
                    Data = CMSMainCat
                });

            }
        }

        private JsonContent GetJsonBrandInShop(int BrandId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var CMSBrandInShop = db.CMSBrandInShops.Where(c => c.CMSId == BrandId).ToList();
                return new JsonContent(new
                {
                    Data = CMSBrandInShop
                });

            }
        }
    }
}