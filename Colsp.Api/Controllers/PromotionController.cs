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
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace Colsp.Api.Controllers
{
    public class PromotionController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        [Route("api/Promotion/OntopCreate")]
        [HttpPost]
        public async Task<HttpResponseMessage> OntopCreate(OnTopCreditCardRequest request)
        {
            PromotionOnTopCreditCard OnTop = null;
            using (var dbcxtransaction = db.Database.BeginTransaction())
            {
                try
                {
                    OnTop = new PromotionOnTopCreditCard();
                    OnTop.CreateBy = this.User.UserRequest().Email;
                    OnTop.CreateOn = DateTime.Now;
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
                    OnTop.UpdateBy = this.User.UserRequest().Email;
                    OnTop.UpdateOn = DateTime.Now;
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
                            card.CreateBy = this.User.UserRequest().Email;
                            card.CreateOn = DateTime.Now;
                            card.UpdateBy = this.User.UserRequest().Email;
                            card.UpdateOn = DateTime.Now;
                            card.Status = val.Status;
                            db.PromotionOnTopCreditNumbers.Add(card);
                        }
                        db.SaveChanges();
                        /*Commit*/
                        dbcxtransaction.Commit();


                        /*
                        Test call api    
                        */
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://devmkp-api.cenergy.co.th/");
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // HTTP GET
                            HttpResponseMessage response = await client.GetAsync("paymentpromotion");
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsAsync<IBMPromotionsList>();

                            }

                            //// HTTP POST
                            //var gizmo = new Product() { Name = "Gizmo", Price = 100, Category = "Widget" };
                            //response = await client.PostAsJsonAsync("api/products", gizmo);
                            //if (response.IsSuccessStatusCode)
                            //{
                            //    Uri gizmoUrl = response.Headers.Location;

                            //    // HTTP PUT
                            //    gizmo.Price = 80;   // Update price
                            //    response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

                            //    // HTTP DELETE
                            //    response = await client.DeleteAsync(gizmoUrl);
                            //}
                        }
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
                        OnTop.CreateBy = this.User.UserRequest().Email;
                        OnTop.CreateOn = DateTime.Now;
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
                        OnTop.UpdateBy = this.User.UserRequest().Email;
                        OnTop.UpdateOn = DateTime.Now;
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
                                        card.CreateBy = this.User.UserRequest().Email;
                                        card.CreateOn = DateTime.Now;
                                        card.UpdateBy = this.User.UserRequest().Email;
                                        card.UpdateOn = DateTime.Now;
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


        [HttpGet]
        [Route("api/Promotion/Buy1Get1")]
        public HttpResponseMessage GetAllBuy1Get1([FromUri] CMSMasterRequest request)
        {
            try
            {

                var query = from pro in db.PromotionBuy1Get1Item select pro;

                if (!string.IsNullOrEmpty(request.SearchText))
                    query = query.Where(x => x.NameEN.Contains(request.SearchText) || x.NameTH.Contains(request.SearchText));

                var total = query.Count();
                var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/Promotion/Buy1Get1");
            }

        }

        [Route("api/Promotion/Buy1Get1/Create")]
        [HttpPost]
        public HttpResponseMessage CreateBuy1Get1Item(Buy1Get1ItemRequest Model)
        {
            PromotionBuy1Get1Item newObj = null;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        newObj = new PromotionBuy1Get1Item();

                        newObj.NameEN = Model.NameEN;
                        newObj.NameTH = Model.NameTH;
                        newObj.URLKey = Model.URLKey;
                        //Check Pid
                        if (Model.ProductBuyList.Count() > 0)
                        {
                            foreach (var item in Model.ProductBuyList)
                            {
                                if (IsCanSetProductBuy(item.Pid) == true)
                                    newObj.PIDBuy = item.Pid;
                            }
                        }
                        if (Model.ProductGetList.Count() > 0)
                        {
                            foreach (var item in Model.ProductBuyList)
                            {
                                if (IsCanSetProductBuy(item.Pid) == true)
                                    newObj.PIDBuy = item.Pid;
                            }
                        }

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
                        newObj.CreateOn = (DateTime)DateTime.Now;
                        newObj.UpdateBy = Model.UpdateBy;
                        newObj.UpdateOn = (DateTime)DateTime.Now;
                        newObj.CreateIP = Model.CreateIP;
                        newObj.UpdateIP = Model.UpdateIP;
                        newObj.CMSStatusFlowId = Model.CMSStatusFlowId;
                        newObj.CampaignID = Model.CampaignID;
                        newObj.CampaignName = Model.CampaignName;
                        newObj.PromotionCode = Model.PromotionCode;
                        newObj.PromotionCodeRef = Model.PromotionCodeRef;
                        newObj.MarketingAbsorb = Model.MarketingAbsorb;
                        newObj.MerchandiseAbsorb = Model.MerchandiseAbsorb;
                        newObj.VendorAbsorb = Model.VendorAbsorb;
                        db.PromotionBuy1Get1Item.Add(newObj);

                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            result = newObj.PromotionBuy1Get1ItemId;
                        }
                        else
                        {
                            dbcxtransaction.Rollback();
                        }
                        return GetBuy1Get1(newObj.PromotionBuy1Get1ItemId);
                    }
                    catch (DbUpdateException e)
                    {
                        dbcxtransaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                    }
                }
            }
        }

        [Route("api/Promotion/Buy1Get1/Update/{PromotionBuy1Get1ItemId}")]
        [HttpPost]
        public HttpResponseMessage EditBuy1Get1Item(Buy1Get1ItemRequest Model)
        {
            PromotionBuy1Get1Item Obj = null;
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        Obj = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Model.PromotionBuy1Get1ItemId).FirstOrDefault();
                        if (Obj != null)
                        {
                            Obj.NameEN = Model.NameEN;
                            Obj.NameTH = Model.NameTH;
                            Obj.URLKey = Model.URLKey;
                            Obj.PIDBuy = Model.PIDBuy;
                            Obj.PIDGet = Model.PIDGet;
                            Obj.ShortDescriptionTH = Model.ShortDescriptionTH;
                            Obj.LongDescriptionTH = Model.LongDescriptionTH;
                            Obj.ShortDescriptionEN = Model.ShortDescriptionEN;
                            Obj.LongDescriptionEN = Model.LongDescriptionEN;
                            Obj.EffectiveDate = Model.EffectiveDate;
                            Obj.EffectiveTime = Model.EffectiveTime;
                            Obj.ExpiryDate = Model.ExpiryDate;
                            Obj.ExpiryTime = Model.ExpiryTime;
                            Obj.ProductBoxBadge = Model.ProductBoxBadge;
                            Obj.Sequence = Model.Sequence;
                            Obj.Status = Model.Status;
                            Obj.CreateBy = Model.CreateBy;
                            Obj.CreateOn = (DateTime)DateTime.Now;
                            Obj.UpdateBy = Model.UpdateBy;
                            Obj.UpdateOn = (DateTime)DateTime.Now;
                            Obj.CreateIP = Model.CreateIP;
                            Obj.UpdateIP = Model.UpdateIP;
                            Obj.CMSStatusFlowId = Model.CMSStatusFlowId;
                            Obj.CampaignID = Model.CampaignID;
                            Obj.CampaignName = Model.CampaignName;
                            Obj.PromotionCode = Model.PromotionCode;
                            Obj.PromotionCodeRef = Model.PromotionCodeRef;
                            Obj.MarketingAbsorb = Model.MarketingAbsorb;
                            Obj.MerchandiseAbsorb = Model.MerchandiseAbsorb;
                            Obj.VendorAbsorb = Model.VendorAbsorb;
                            db.Entry(Obj).State = EntityState.Modified;

                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = Obj.PromotionBuy1Get1ItemId;
                            }
                            else
                            {
                                dbcxtransaction.Rollback();
                            }
                        }
                        return GetBuy1Get1(Obj.PromotionBuy1Get1ItemId);
                    }
                    catch (DbUpdateException e)
                    {
                        dbcxtransaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
                    }
                }
            }
        }

        [Route("api/Promotion/Buy1Get1/{PromotionBuy1Get1ItemId}")]
        [HttpGet]
        private HttpResponseMessage GetBuy1Get1(int promotionBuy1Get1ItemId)
        {
            try
            {
                Buy1Get1ItemResponse buy1get1 = GetBuy1Get1Response(promotionBuy1Get1ItemId);
                if (buy1get1 == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, buy1get1);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private Buy1Get1ItemResponse GetBuy1Get1Response(int promotionBuy1Get1ItemId)
        {
            using (ColspEntities db = new ColspEntities())
            {
                var buy1get1 = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == promotionBuy1Get1ItemId).FirstOrDefault();
                if (buy1get1 != null)
                {
                    Buy1Get1ItemResponse result = new Buy1Get1ItemResponse();
                    result.NameEN = buy1get1.NameEN;
                    result.NameTH = buy1get1.NameTH;
                    result.URLKey = buy1get1.URLKey;
                    result.PIDBuy = buy1get1.PIDBuy;
                    result.PIDGet = buy1get1.PIDGet;
                    result.ShortDescriptionTH = buy1get1.ShortDescriptionTH;
                    result.LongDescriptionTH = buy1get1.LongDescriptionTH;
                    result.ShortDescriptionEN = buy1get1.ShortDescriptionEN;
                    result.LongDescriptionEN = buy1get1.LongDescriptionEN;
                    result.EffectiveDate = buy1get1.EffectiveDate;
                    result.EffectiveTime = buy1get1.EffectiveTime;
                    result.ExpiryDate = buy1get1.ExpiryDate;
                    result.ExpiryTime = buy1get1.ExpiryTime;
                    result.ProductBoxBadge = buy1get1.ProductBoxBadge;
                    result.Sequence = buy1get1.Sequence;
                    result.Status = buy1get1.Status;
                    result.CreateBy = buy1get1.CreateBy;
                    result.CreateOn = (DateTime)DateTime.Now;
                    result.UpdateBy = buy1get1.UpdateBy;
                    result.UpdateOn = (DateTime)DateTime.Now;
                    result.CreateIP = buy1get1.CreateIP;
                    result.UpdateIP = buy1get1.UpdateIP;
                    result.CMSStatusFlowId = buy1get1.CMSStatusFlowId;
                    result.CampaignID = buy1get1.CampaignID;
                    result.CampaignName = buy1get1.CampaignName;
                    result.PromotionCode = buy1get1.PromotionCode;
                    result.PromotionCodeRef = buy1get1.PromotionCodeRef;
                    result.MarketingAbsorb = buy1get1.MarketingAbsorb;
                    result.MerchandiseAbsorb = buy1get1.MerchandiseAbsorb;
                    result.VendorAbsorb = buy1get1.VendorAbsorb;
                    return result;
                }
                else
                {
                    return null;
                }
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
                                    UpdateDate = (DateTime)p.UpdateOn,
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
                    //OnTopCreditCard.CreateBy = (int)OnTop.CreateBy;
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

        #region find data function
        public bool IsCanSetProductBuy(int Pid)
        {
            /*
            1.Has Pid in table ?
            2.It Has Promotion Expire?
            */
            bool result = false;
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    var findPidBuy = db.PromotionBuy1Get1Item.Where(c => c.PIDBuy == Pid).ToList();
                    if (findPidBuy.Count() > 0)
                    {
                        foreach (var item in findPidBuy)
                        {
                            if (item.ExpiryDate < DateTime.Today)
                                result = true;
                            else
                                result = false;
                        }

                    }
                    else result = true;
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool IsCanSetProductGet(int Pid)
        {
            /*
            1.Has Pid in table ?
            2.It Has Promotion Expire?
            */
            bool result = false;
            try
            {
                using (ColspEntities db = new ColspEntities())
                {
                    var findPidGet = db.PromotionBuy1Get1Item.Where(c => c.PIDGet == Pid).ToList();
                    if (findPidGet.Count() > 0)
                    {
                        foreach (var item in findPidGet)
                        {
                            if (item.ExpiryDate < DateTime.Today)
                                result = true;
                            else
                                result = false;
                        }

                    }
                    else result = true;
                }
                return result;
            }
            catch (Exception)
            {

                throw;
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

public class IBMPromotionsList
{
    public List<Paymentpromotion> paymentpromotions { get; set; }
    public string returncode { get; set; }
    public Message message { get; set; }
}

public class Message
{
    public string samplestring1 { get; set; }
    public string samplestring3 { get; set; }
}

public class Paymentpromotion
{
    public string id { get; set; }
    public string promotioncode { get; set; }
    public DateTime effectivedate { get; set; }
    public DateTime expireddate { get; set; }
    public string[] creditcardcodes { get; set; }
    public float discountbaht { get; set; }
    public float discountpercent { get; set; }
    public float maximumdiscount { get; set; }
    public float minimumorderamount { get; set; }
}
