using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Helpers;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
    public class PromotionController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        [Route("api/Promotion/OntopCreate")]
        [HttpPost]
        public HttpResponseMessage OntopCreate(OnTopCreditCardRequest request)
        {
            PromotionOnTopCreditCard OnTop = null;
            using (var dbcxtransaction = db.Database.BeginTransaction())
            {
                try
                {
                    OnTop = new PromotionOnTopCreditCard();
                    OnTop.CreateBy = this.User.UserRequest().UserId;
                    OnTop.Createdate = DateTime.Now;
                    OnTop.BankNameEN = request.BankNameEN;
                    OnTop.BankNameTH = request.BankNameTH;
                    OnTop.CreateIP = request.CreateIP;
                    OnTop.DiscountType = request.DiscountType;
                    OnTop.DiscountValue = request.DiscountValue;
                    DateTime EffectiveDate = new DateTime();
                    DateTime ExpiryDate = new DateTime();
                    if (!string.IsNullOrWhiteSpace(request.EffectiveDate))
                    {
                        if (!DateTime.TryParse(request.EffectiveDate, out EffectiveDate))
                        {
                            // handle parse failure
                            dbcxtransaction.Rollback();
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                        }

                    }
                    if (!string.IsNullOrWhiteSpace(request.ExpiryDate))
                    {
                        if (!DateTime.TryParse(request.ExpiryDate, out ExpiryDate))
                        {
                            // handle parse failure
                            dbcxtransaction.Rollback();
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                        }
                    }

                    OnTop.EffectiveDate = EffectiveDate.Date;
                    OnTop.EffectiveTime = EffectiveDate.TimeOfDay;
                    OnTop.ExpiryDate = ExpiryDate.Date;
                    OnTop.ExpiryTime = ExpiryDate.TimeOfDay;
                    OnTop.FreeShipping = request.FreeShipping;
                    OnTop.IconURLEN = request.IconURLEN;
                    OnTop.IconURLTH = request.IconURLTH;
                    OnTop.MaximumDiscountAmount = request.MaximumDiscountAmount;
                    OnTop.MinimumOrderAmount = request.MinimumOrderAmount;
                    OnTop.NameEN = request.NameEN;
                    OnTop.NameTH = request.NameTH;
                    OnTop.PaymantId = request.PaymentId;
                    OnTop.PromotionCode = request.PromotionCode;
                    OnTop.Sequence = request.Sequence;
                    OnTop.ShopId = request.ShopId;
                    OnTop.ShortDescriptionEN = request.ShortDescriptionEN;
                    OnTop.ShortDescriptionTH = request.ShortDescriptionTH;
                    if (request.Status == Constant.STATUS_PROMOTION_ACTIVE)
                        OnTop.Status = true;
                    else
                        OnTop.Status = false;
                    OnTop.UpdateIP = request.CreateIP;
                    OnTop.UpdateBy = this.User.UserRequest().UserId;
                    OnTop.UpdateDate = DateTime.Now;
                    OnTop.Visibility = request.Visibility;
                    OnTop = db.PromotionOnTopCreditCards.Add(OnTop);
                    if (db.SaveChanges() > 0)
                    {
                        foreach (var val in request.CardItemList)
                        {
                            PromotionOnTopCreditNumber card = new PromotionOnTopCreditNumber();
                            card.OnTopCreditCardId = OnTop.OnTopCreditCardId;
                            card.CreditCardTypeCode = val.CreditCardTypeCode;
                            card.CreditNumberFormat = val.CreditNumberFormat;
                            card.Digit = val.Digit;
                            card.Visibility = val.Visibility;
                            card.CreateBy = this.User.UserRequest().UserId;
                            card.Createdate = DateTime.Now;
                            card.UpdateBy = this.User.UserRequest().UserId;
                            card.UpdateDate = DateTime.Now;
                            card.Status = val.Status;
                            db.PromotionOnTopCreditNumbers.Add(card);
                        }
                        db.SaveChanges();
                        /*Commit*/
                        dbcxtransaction.Commit();

                    }
                    else
                    {
                        dbcxtransaction.Rollback();
                    }
                    return GetOnTopCreditCard(OnTop.OnTopCreditCardId);
                }
                catch (DbUpdateException e)
                {
                    dbcxtransaction.Rollback();
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                }
            }
        }

        [Route("api/Promotion/OntopUpdate/{OnTopCreditCardId}")]
        [HttpPut]
        public HttpResponseMessage OntopUpdate([FromUri] int OnTopCreditCardId, OnTopCreditCardRequest request)
        {
            var OnTop = db.PromotionOnTopCreditCards.Where(c => c.OnTopCreditCardId == OnTopCreditCardId).FirstOrDefault();
            if (OnTop != null)
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        OnTop.CreateBy = this.User.UserRequest().UserId;
                        OnTop.Createdate = DateTime.Now;
                        OnTop.BankNameEN = request.BankNameEN;
                        OnTop.BankNameTH = request.BankNameTH;
                        OnTop.CreateIP = request.CreateIP;
                        OnTop.DiscountType = request.DiscountType;
                        OnTop.DiscountValue = request.DiscountValue;
                        DateTime EffectiveDate = new DateTime();
                        DateTime ExpiryDate = new DateTime();
                        if (!string.IsNullOrWhiteSpace(request.EffectiveDate))
                        {
                            if (!DateTime.TryParse(request.EffectiveDate, out EffectiveDate))
                            {
                                // handle parse failure
                                dbcxtransaction.Rollback();
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                            }

                        }
                        if (!string.IsNullOrWhiteSpace(request.ExpiryDate))
                        {
                            if (!DateTime.TryParse(request.ExpiryDate, out ExpiryDate))
                            {
                                // handle parse failure
                                dbcxtransaction.Rollback();
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                            }
                        }

                        OnTop.EffectiveDate = EffectiveDate.Date;
                        OnTop.EffectiveTime = EffectiveDate.TimeOfDay;
                        OnTop.ExpiryDate = ExpiryDate.Date;
                        OnTop.ExpiryTime = ExpiryDate.TimeOfDay;
                        OnTop.FreeShipping = request.FreeShipping;
                        OnTop.IconURLEN = request.IconURLEN;
                        OnTop.IconURLTH = request.IconURLTH;
                        OnTop.MaximumDiscountAmount = request.MaximumDiscountAmount;
                        OnTop.MinimumOrderAmount = request.MinimumOrderAmount;
                        OnTop.NameEN = request.NameEN;
                        OnTop.NameTH = request.NameTH;
                        OnTop.PaymantId = request.PaymentId;
                        OnTop.PromotionCode = request.PromotionCode;
                        OnTop.Sequence = request.Sequence;
                        OnTop.ShopId = request.ShopId;
                        OnTop.ShortDescriptionEN = request.ShortDescriptionEN;
                        OnTop.ShortDescriptionTH = request.ShortDescriptionTH;
                        if (request.Status == Constant.STATUS_PROMOTION_ACTIVE)
                            OnTop.Status = true;
                        else
                            OnTop.Status = false;
                        OnTop.UpdateIP = request.CreateIP;
                        OnTop.UpdateBy = this.User.UserRequest().UserId;
                        OnTop.UpdateDate = DateTime.Now;
                        OnTop.Visibility = request.Visibility;

                        db.Entry(OnTop).State = EntityState.Modified;
                        if (db.SaveChanges() > 0)
                        {
                            if (request.CardItemList != null && request.CardItemList.Count() > 0)
                            {
                                foreach (var val in request.CardItemList)
                                {
                                    var card = db.PromotionOnTopCreditNumbers.Where(c => c.OnTopCreditCardId == OnTop.OnTopCreditCardId && c.OnTopCreditNumberId == val.OnTopCreditNumberId).FirstOrDefault();
                                    if (card != null)
                                    {
                                        card.OnTopCreditCardId = OnTop.OnTopCreditCardId;
                                        card.CreditCardTypeCode = val.CreditCardTypeCode;
                                        card.CreditNumberFormat = val.CreditNumberFormat;
                                        card.Digit = val.Digit;
                                        card.Visibility = val.Visibility;
                                        card.CreateBy = this.User.UserRequest().UserId;
                                        card.Createdate = DateTime.Now;
                                        card.UpdateBy = this.User.UserRequest().UserId;
                                        card.UpdateDate = DateTime.Now;
                                        card.Status = val.Status;
                                        db.Entry(card).State = EntityState.Modified;
                                    }
                                }
                                db.SaveChanges();

                            }
                            /*Commit*/
                            dbcxtransaction.Commit();

                        }
                        else
                        {
                            dbcxtransaction.Rollback();
                        }
                        return GetOnTopCreditCard(OnTop.OnTopCreditCardId);
                    }
                    catch (DbUpdateException e)
                    {
                        dbcxtransaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                    }
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
            }
        }



        [Route("api/Promotion/Ontopcredit/{OnTopCreditCardId}")]
        [HttpGet]
        public HttpResponseMessage GetOnTopCreditCard(int OnTopCreditCardId)
        {
            try
            {
                OnTopCreditCardResponse OnTopCreditCard = GetOnTopCreditCardResponse(OnTopCreditCardId);
                if (OnTopCreditCard == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, OnTopCreditCard);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        #region List
        [Route("api/Promotion/Ontopcredit")]
        [HttpGet]
        public HttpResponseMessage GetOntopcredit([FromUri] OnTopCreditCardListRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var products = (from p in db.PromotionOnTopCreditCards
                                select new OnTopCreditCardListResponse
                                {
                                    OnTopCreditCardId = p.OnTopCreditCardId,
                                    NameEN = p.NameEN,
                                    NameTH = p.NameTH,
                                    BankNameEN = p.BankNameEN,
                                    BankNameTH = p.BankNameTH,
                                    MinimumOrderAmount = (decimal)p.MinimumOrderAmount,
                                    MaximumDiscountAmount = (decimal)p.MaximumDiscountAmount,
                                    Status = p.Status == true ? Constant.STATUS_PROMOTION_ACTIVE : Constant.STATUS_PROMOTION_INACTIVE,
                                    Visibility = (bool)p.Visibility,
                                    UpdateDate = (DateTime)p.UpdateDate,
                                    ShopId = (int)p.ShopId,
                                    DiscountType = p.DiscountType
                                });
                if (this.User.HasPermission("View Product"))
                {
                    int shopId = this.User.ShopRequest().ShopId;
                    products = products.Where(w => w.ShopId == shopId);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.NameEN.Contains(request.SearchText)
                    || p.NameTH.Contains(request.SearchText));
                }

                var total = products.Count();
                var pagedProducts = products.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private OnTopCreditCardResponse GetOnTopCreditCardResponse(int OnTopCreditCardId)
        {
            using (ColspEntities dbx = new ColspEntities())
            {
                var OnTop = dbx.PromotionOnTopCreditCards.Where(w => w.OnTopCreditCardId == OnTopCreditCardId).FirstOrDefault();
                if (OnTop != null)
                {
                    OnTopCreditCardResponse OnTopCreditCard = new OnTopCreditCardResponse();
                    OnTopCreditCard.BankNameEN = OnTop.BankNameEN;
                    OnTopCreditCard.BankNameTH = OnTop.BankNameTH;
                    OnTopCreditCard.CreateBy = (int)OnTop.CreateBy;
                    OnTopCreditCard.CreateIP = OnTop.CreateIP;
                    OnTopCreditCard.DiscountType = OnTop.DiscountType;
                    OnTopCreditCard.DiscountValue = (decimal)OnTop.DiscountValue;
                    OnTopCreditCard.EffectiveDate = (DateTime)OnTop.EffectiveDate;
                    OnTopCreditCard.EffectiveTime = (TimeSpan)OnTop.EffectiveTime;
                    OnTopCreditCard.ExpiryDate = (DateTime)OnTop.ExpiryDate;
                    OnTopCreditCard.ExpiryTime = (TimeSpan)OnTop.ExpiryTime;
                    OnTopCreditCard.FreeShipping = (bool)OnTop.FreeShipping;
                    OnTopCreditCard.IconURLEN = OnTop.IconURLEN;
                    OnTopCreditCard.IconURLTH = OnTop.IconURLTH;
                    OnTopCreditCard.MinimumOrderAmount = (decimal)OnTop.MinimumOrderAmount;
                    OnTopCreditCard.MaximumDiscountAmount = (decimal)OnTop.MaximumDiscountAmount;
                    OnTopCreditCard.NameEN = OnTop.NameEN;
                    OnTopCreditCard.NameTH = OnTop.NameTH;
                    OnTopCreditCard.OnTopCreditCardId = OnTop.OnTopCreditCardId;
                    OnTopCreditCard.PromotionCode = OnTop.PromotionCode;
                    OnTopCreditCard.PaymentId = OnTop.PaymantId;
                    OnTopCreditCard.Sequence = OnTop.Sequence;
                    OnTopCreditCard.ShopId = OnTop.ShopId;
                    OnTopCreditCard.Status = OnTop.Status == true ? Constant.STATUS_PROMOTION_ACTIVE : Constant.STATUS_PROMOTION_INACTIVE;
                    OnTopCreditCard.ShortDescriptionEN = OnTop.ShortDescriptionEN;
                    OnTopCreditCard.ShortDescriptionTH = OnTop.ShortDescriptionTH;
                    OnTopCreditCard.Visibility = (bool)OnTop.Visibility;
                    List<Carditemlist> OnTopCardList = new List<Carditemlist>();
                    var CardList = db.PromotionOnTopCreditNumbers.Where(c => c.OnTopCreditCardId == OnTopCreditCardId).ToList();
                    if (CardList != null && CardList.Count() > 0)
                    {
                        foreach (var card in CardList)
                        {
                            Carditemlist val = new Carditemlist();
                            val.CreditCardTypeCode = card.CreditCardTypeCode;
                            val.CreditNumberFormat = card.CreditNumberFormat;
                            val.Digit = (int)card.Digit;
                            val.OnTopCreditCardId = (int)card.OnTopCreditCardId;
                            val.Status = (bool)card.Status;
                            val.Visibility = (bool)card.Visibility;
                            OnTopCardList.Add(val);
                        }
                    }
                    OnTopCreditCard.CardItemList = OnTopCardList;

                    return OnTopCreditCard;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
