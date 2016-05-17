using Colsp.Api.CMSFunction;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.ByOneGetOneFunction
{
    public class ByOneGetOneProcess
    {
        #region Create
        public int CreateBy1GetItem(By1Get1ItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        PromotionBy1Get1Item bg = new PromotionBy1Get1Item();
                        bg.NameEN = Model.NameEN;
                        bg.NameTH = Model.NameTH;
                        bg.EffectiveDate = Model.EffectiveDate;
                        bg.EffectiveTime = Model.EffectiveTime;
                        bg.ExpiryDate = Model.ExpiryDate;
                        bg.ExpiryTime = Model.ExpiryTime;
                        bg.ShortDescriptionEN = Model.ShortDescriptionEN;
                        bg.ShortDescriptionTH = Model.ShortDescriptionTH;
                        bg.ShopId = Model.ShopId;
                        bg.ShortDetailEN = Model.ShortDetailEN;
                        bg.ShortDetailTH = Model.ShortDetailTH;
                        bg.Status = Model.Status;
                        bg.ByPID = Model.ByPID;
                        bg.GetPID = Model.GetPID;
                        bg.URLKey = Model.URLKey;
                        bg.Visibility = Model.Visibility;
                        bg.CreateBy = Model.CreateBy;
                        bg.Createdate = DateTime.Now;
                        bg.CreateIP = Model.CreateIP;
                        db.PromotionBy1Get1Item.Add(bg);
                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            ////History Log
                            HistoryLogClass log = new HistoryLogClass();
                            log.LogCreateCMS(bg.PromotionBy1Get1ItemId, "PromotionBy1Get1Item", bg.Status, "Create", (int)bg.CreateBy, bg.CreateIP);
                            result = bg.PromotionBy1Get1ItemId;
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
    }

}