using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colsp.Model.Request;
using Colsp.Entity.Models;
using System.Data.Entity;

namespace Colsp.Api.ByOneGetOneFunction
{
    public class BuyOneGetOneProcess
    {
        #region create
        //CreateBuy1GetItem by sending Model
        public int CreateBuy1Get1Item(Buy1Get1ItemRequest Model)
        {
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        PromotionBuy1Get1Item newObj = new PromotionBuy1Get1Item();
                        
                        newObj.NameEN = Model.NameEN;
                        newObj.NameTH = Model.NameTH;
                        newObj.URLKey = Model.URLKey;
                        newObj.PIDBuy = Model.PIDBuy;
                        newObj.PIDGet = Model.PIDGet;
                        newObj.ShortDescriptionTH = Model.ShortDescriptionTH;
                        newObj.LongDescriptionTH = Model.LongDescriptionTH;
                        newObj.ShortDescriptionEN = Model.ShortDescriptionEN;
                        newObj.LongDescriptionEN = Model.LongDescriptionEN;
                        newObj.EffectiveDate = Model.EffectiveDate;
                        newObj.EffectiveTime = Model.EffectiveTime;
                        newObj.ExpiryDate = Model.ExpiryDate;
                        newObj.ExpiryTime = Model.ExpiryTime;
                        newObj.ProductBoxBadge = Model.ProductBoxBadge;
                        newObj.Sequence = Model.Sequence;
                        newObj.Status = Model.Status;
                        newObj.CreateBy = Model.CreateBy;
                        newObj.Createdate = (DateTime)DateTime.Now;
                        newObj.UpdateBy = Model.UpdateBy;
                        newObj.UpdateDate = (DateTime)DateTime.Now;
                        newObj.CreateIP = Model.CreateIP;
                        newObj.UpdateIP = Model.UpdateIP;
                        newObj.CMSStatusFlowId = Model.CMSStatusFlowId;
                        newObj.Visibility = Model.Visibility;
                        db.PromotionBuy1Get1Item.Add(newObj);

                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            result = newObj.PromotionBuy1Get1ItemId;
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

        #region update
        //Update Only Sending, Delete (Status = 0;false )
        public int UpdateProBuy1Get1(Buy1Get1ItemRequest Model)
        {
            //var modelItem = Model;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    //foreach (var modelItem in Model) {
                    try
                    {
                        var newObj = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Model.PromotionBuy1Get1ItemId).FirstOrDefault();
                        if (newObj != null)
                        {
                            newObj.NameEN                       = Model.NameEN != default(string) ? Model.NameEN : newObj.NameEN ;
                            newObj.NameTH                       = Model.NameTH != default(string) ? Model.NameTH : newObj.NameTH;
                            newObj.URLKey                       = Model.URLKey != default(string) ? Model.URLKey : newObj.URLKey;
                            newObj.PIDBuy                       = Model.PIDBuy != default(int) ? Model.PIDBuy : newObj.PIDBuy;
                            newObj.PIDGet                       = Model.PIDGet != default(int) ? Model.PIDGet : newObj.PIDGet;
                            newObj.ShortDescriptionTH           = Model.ShortDescriptionTH != default(string) ? Model.ShortDescriptionTH : newObj.ShortDescriptionTH;
                            newObj.LongDescriptionTH            = Model.LongDescriptionTH != default(string) ? Model.LongDescriptionTH : newObj.LongDescriptionTH;
                            newObj.ShortDescriptionEN           = Model.ShortDescriptionEN != default(string) ? Model.ShortDescriptionEN : newObj.ShortDescriptionEN;
                            newObj.LongDescriptionEN            = Model.LongDescriptionEN != default(string) ? Model.LongDescriptionEN : newObj.LongDescriptionEN;
                            newObj.EffectiveDate                = Model.EffectiveDate ?? newObj.EffectiveDate;
                            newObj.EffectiveTime                = Model.EffectiveTime ?? newObj.EffectiveTime;
                            newObj.ExpiryDate                   = Model.ExpiryDate ?? newObj.ExpiryDate;
                            newObj.ExpiryTime                   = Model.ExpiryTime ?? newObj.ExpiryTime;
                            newObj.ProductBoxBadge              = Model.ProductBoxBadge != default(string) ? Model.ProductBoxBadge : newObj.ProductBoxBadge;
                            newObj.Sequence                     = Model.Sequence ?? newObj.Sequence;
                            newObj.Status                       = Model.Status ?? newObj.Status;
                            newObj.CreateBy                     = Model.CreateBy ?? newObj.CreateBy;
                            newObj.Createdate                   = Model.Createdate != null ? (DateTime)Model.Createdate : newObj.Createdate ;
                            newObj.UpdateBy                     = Model.UpdateBy ?? newObj.UpdateBy;
                            newObj.UpdateDate                   = (DateTime)DateTime.Now  ;
                            newObj.CreateIP                     = Model.CreateIP != default(string) ? Model.CreateIP : newObj.CreateIP;
                            newObj.UpdateIP                     = Model.UpdateIP != default(string) ? Model.UpdateIP : newObj.UpdateIP;
                            newObj.CMSStatusFlowId              = Model.CMSStatusFlowId.HasValue ? Model.CMSStatusFlowId : newObj.CMSStatusFlowId;
                            newObj.Visibility                   = Model.Visibility.HasValue ? Model.Visibility : newObj.Visibility;

                            db.Entry(newObj).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = newObj.PromotionBuy1Get1ItemId;
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

        //Edit All Sending
        public int EditProBuy1Get1(Buy1Get1ItemRequest Model)
        {
            //var modelItem = Model;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    //foreach (var modelItem in Model) {
                    try
                    {
                        var newObj = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Model.PromotionBuy1Get1ItemId).FirstOrDefault();
                        if (newObj != null)
                        {
                            newObj.NameEN = Model.NameEN ;
                            newObj.NameTH = Model.NameTH ;
                            newObj.URLKey = Model.URLKey ;
                            newObj.PIDBuy = Model.PIDBuy;
                            newObj.PIDGet = Model.PIDGet;
                            newObj.ShortDescriptionTH = Model.ShortDescriptionTH;
                            newObj.LongDescriptionTH = Model.LongDescriptionTH;
                            newObj.ShortDescriptionEN = Model.ShortDescriptionEN;
                            newObj.LongDescriptionEN = Model.LongDescriptionEN;
                            newObj.EffectiveDate = Model.EffectiveDate;
                            newObj.EffectiveTime = Model.EffectiveTime;
                            newObj.ExpiryDate = Model.ExpiryDate;
                            newObj.ExpiryTime = Model.ExpiryTime;
                            newObj.ProductBoxBadge = Model.ProductBoxBadge;
                            newObj.Sequence = Model.Sequence;
                            newObj.Status = Model.Status;
                            newObj.CreateBy = Model.CreateBy;
                            newObj.Createdate = Model.Createdate != null ? (DateTime)Model.Createdate : newObj.Createdate;
                            newObj.UpdateBy = Model.UpdateBy;
                            newObj.UpdateDate = (DateTime)DateTime.Now;
                            newObj.CreateIP = Model.CreateIP;
                            newObj.UpdateIP = Model.UpdateIP;
                            newObj.CMSStatusFlowId = Model.CMSStatusFlowId;
                            newObj.Visibility = Model.Visibility;

                            db.Entry(newObj).State = EntityState.Modified;
                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = newObj.PromotionBuy1Get1ItemId;
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

        #endregion
    }
}