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
using System.Text;
using Colsp.Logic;

namespace Colsp.Api.Controllers
{
    public class PromotionController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        #region Properties

        public class Buy1Get1CallModel
        {
            public string id { get; set; }
            public string promotioncode { get; set; }
            public DateTime effectivedate { get; set; }
            public DateTime expireddate { get; set; }
            public bool status { get; set; }
            public List<Buyproduct> buyproducts { get; set; }
            public List<Premiumproduct> premiumproducts { get; set; }
            public int limitperuser { get; set; }
            public int limit { get; set; }
        }

        public class Buyproduct
        {
            public string productid { get; set; }
            public int quantity { get; set; }
        }

        public class Premiumproduct
        {
            public string productid { get; set; }
            public int quantity { get; set; }
        }



        public class Buy1Get1ReturnModel
        {
            public Premium premium { get; set; }
            public string returncode { get; set; }
            public string message { get; set; }
        }

        public class Premium
        {
            public string id { get; set; }
            public string promotioncode { get; set; }
            public DateTime effectivedate { get; set; }
            public DateTime expireddate { get; set; }
            public bool status { get; set; }
            public List<Buyproduct> buyproduct { get; set; }
            public List<Premiumproduct> premiumproduct { get; set; }
            public int limitperuser { get; set; }
            public int limit { get; set; }
        }


        public class SearchBuy1Get1ItemRequest : PaginatedRequest
        {
            public string SearchText { get; set; }
        }

        public class Buy1Get1ReturnMongo
        {
            public string _status { get; set; }
            public string _Id { get; set; }
            public string _message { get; set; }
        }

        public class PaymentIdModel
        {
            public int PaymentID { get; set; }
            public string PaymentName { get; set; }
            public string PaymentNameEN { get; set; }
        }
        #endregion
        #region API Mongo
        static async void CallMongoAPI(Buy1Get1CallModel data, string Id, string Method, string FunctionName, Buy1Get1ReturnModel mongoRetrun)
        {
            /*
            Test call api    
            */
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://devmkp-api.cenergy.co.th/");
                byte[] cred = UTF8Encoding.UTF8.GetBytes("mkp:mkp@shopping");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = new HttpResponseMessage();
                dynamic result;
                switch (Method)
                {
                    case "GET":
                        // HTTP GET
                        response = await client.GetAsync(FunctionName);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<Buy1Get1ReturnModel>();
                        }
                        break;
                    case "GETID":
                        // HTTP GET By Id
                        response = await client.GetAsync(FunctionName + "?id=" + Id);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<Buy1Get1ReturnModel>();
                        }
                        break;
                    case "POST":
                        response = await client.PostAsJsonAsync(FunctionName, data);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<Buy1Get1ReturnModel>();
                            mongoRetrun = result;
                        }
                        break;
                    case "PUT":
                        response = await client.PutAsJsonAsync(FunctionName + "?id=" + Id, data);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<Buy1Get1ReturnModel>();
                            mongoRetrun = result;
                        }
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(FunctionName);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<Buy1Get1ReturnModel>();
                            mongoRetrun = result;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region On Top Create

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


        #endregion
        [HttpGet]
        [Route("api/Promotion/Buy1Get1")]
        public HttpResponseMessage GetAllBuy1Get1([FromUri] SearchBuy1Get1ItemRequest request)
        {
            try
            {
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var query = from pro in db.PromotionBuy1Get1Item where pro.ShopId == ShopId select pro;

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
            Buy1Get1ReturnModel mongoReturn = new Buy1Get1ReturnModel();
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    Buy1Get1CallModel callMongoModel = new Buy1Get1CallModel();
                    List<Buyproduct> buypro = new List<Buyproduct>();
                    List<Premiumproduct> getpro = new List<Premiumproduct>();
                    try
                    {
                        newObj = new PromotionBuy1Get1Item();

                        newObj.NameEN = Model.NameEN;
                        newObj.NameTH = Model.NameTH;
                        newObj.URLKey = Model.URLKey;
                        //Check Pid
                        if (Model.ProductBuyList != null && Model.ProductBuyList.Count() > 0)
                        {
                            foreach (var itemBuy in Model.ProductBuyList)
                            {
                                if (IsCanSetProductBuy(itemBuy.Pid) == true)
                                {
                                    Buyproduct proCall = new Buyproduct();
                                    newObj.PIDBuy = itemBuy.Pid;
                                    proCall.productid = itemBuy.Pid.ToString();
                                    proCall.quantity = 1;
                                    buypro.Add(proCall);
                                }
                            }
                        }
                        if (Model.ProductGetList != null && Model.ProductGetList.Count() > 0)
                        {
                            foreach (var itemGet in Model.ProductGetList)
                            {
                                if (IsCanSetProductBuy(itemGet.Pid) == true)
                                {
                                    Premiumproduct getCall = new Premiumproduct();
                                    newObj.PIDGet = itemGet.Pid;
                                    getCall.productid = itemGet.Pid.ToString();
                                    getCall.quantity = 1;
                                    getpro.Add(getCall);
                                }
                            }
                        }

                        newObj.ShortDescriptionTH = Model.ShortDescriptionTH;
                        newObj.LongDescriptionTH = Model.LongDescriptionTH;
                        newObj.ShortDescriptionEN = Model.ShortDescriptionEN;
                        newObj.LongDescriptionEN = Model.LongDescriptionEN;
                        newObj.EffectiveDate = Model.EffectiveDate;
                        newObj.ExpiryDate = Model.ExpiryDate;
                        newObj.ProductBoxBadge = "";
                        newObj.Sequence = 1;
                        if (Model.Status == 1)
                            newObj.Status = true;
                        else
                            newObj.Status = false;
                        newObj.CreateBy = User.UserRequest() == null ? "" : User.UserRequest().Email;
                        newObj.CreateOn = (DateTime)DateTime.Now;
                        newObj.UpdateBy = User.UserRequest() == null ? "" : User.UserRequest().Email;
                        newObj.UpdateOn = (DateTime)DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(Model.CreateIP))
                            newObj.CreateIP = Model.CreateIP;
                        else
                            newObj.CreateIP = "";
                        newObj.UpdateIP = newObj.CreateIP;
                        if (Model.CMSStatusFlowId.HasValue)
                            newObj.CMSStatusFlowId = Model.CMSStatusFlowId;
                        else
                            newObj.CMSStatusFlowId = 1;
                        newObj.CampaignID = Model.CampaignID;
                        newObj.CampaignName = Model.CampaignName;
                        newObj.PromotionCode = Model.PromotionCode;
                        newObj.PromotionCodeRef = Model.PromotionCodeRef;
                        newObj.MarketingAbsorb = Model.MarketingAbsorb;
                        newObj.MerchandiseAbsorb = Model.MerchandiseAbsorb;
                        newObj.VendorAbsorb = Model.VendorAbsorb;
                        newObj.Limit = Model.Limit;
                        newObj.LimitPerUser = Model.LimitPerUser;
                        newObj.ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                        db.PromotionBuy1Get1Item.Add(newObj);

                        if (db.SaveChanges() > 0) //Saved return row save successfully.
                        {
                            dbcxtransaction.Commit();
                            result = newObj.PromotionBuy1Get1ItemId;

                            callMongoModel.buyproducts = buypro;
                            callMongoModel.effectivedate = (DateTime)Model.EffectiveDate;
                            callMongoModel.expireddate = (DateTime)Model.ExpiryDate;
                            callMongoModel.limit = (int)newObj.Limit;
                            callMongoModel.limitperuser = (int)newObj.LimitPerUser;
                            callMongoModel.premiumproducts = getpro;
                            callMongoModel.status = (bool)newObj.Status;
                            callMongoModel.id = "";
                            callMongoModel.promotioncode = newObj.PromotionCode;
                            CallMongoAPI(callMongoModel, "", "POST", "premium", mongoReturn);
                            var rex = mongoReturn;
                            UpdatePremiumId(newObj.PromotionBuy1Get1ItemId, rex.premium.id);
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
            PromotionBuy1Get1Item newObj = null;
            Buy1Get1ReturnModel mongoReturn = new Buy1Get1ReturnModel();
            int result = 0;
            using (ColspEntities db = new ColspEntities())
            {
                using (var dbcxtransaction = db.Database.BeginTransaction())
                {
                    Buy1Get1CallModel callMongoModel = new Buy1Get1CallModel();
                    List<Buyproduct> buypro = new List<Buyproduct>();
                    List<Premiumproduct> getpro = new List<Premiumproduct>();
                    try
                    {
                        int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                        newObj = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Model.PromotionBuy1Get1ItemId && c.ShopId == ShopId).FirstOrDefault();
                        if (newObj != null)
                        {

                            newObj.NameEN = Model.NameEN;
                            newObj.NameTH = Model.NameTH;
                            newObj.URLKey = Model.URLKey;
                            //Check Pid
                            if (Model.ProductBuyList != null && Model.ProductBuyList.Count() > 0)
                            {
                                foreach (var itemBuy in Model.ProductBuyList)
                                {
                                    if (IsCanSetProductBuy(itemBuy.Pid) == true)
                                    {
                                        Buyproduct proCall = new Buyproduct();
                                        newObj.PIDBuy = itemBuy.Pid;
                                        proCall.productid = itemBuy.Pid.ToString();
                                        proCall.quantity = 1;
                                        buypro.Add(proCall);
                                    }
                                }
                            }
                            if (Model.ProductGetList != null && Model.ProductGetList.Count() > 0)
                            {
                                foreach (var itemGet in Model.ProductGetList)
                                {
                                    if (IsCanSetProductBuy(itemGet.Pid) == true)
                                    {
                                        Premiumproduct getCall = new Premiumproduct();
                                        newObj.PIDGet = itemGet.Pid;
                                        getCall.productid = itemGet.Pid.ToString();
                                        getCall.quantity = 1;
                                        getpro.Add(getCall);
                                    }
                                }
                            }

                            newObj.ShortDescriptionTH = Model.ShortDescriptionTH;
                            newObj.LongDescriptionTH = Model.LongDescriptionTH;
                            newObj.ShortDescriptionEN = Model.ShortDescriptionEN;
                            newObj.LongDescriptionEN = Model.LongDescriptionEN;
                            newObj.EffectiveDate = Model.EffectiveDate;
                            newObj.ExpiryDate = Model.ExpiryDate;
                            newObj.ProductBoxBadge = "";
                            newObj.Sequence = 1;
                            if (Model.Status == 1)
                                newObj.Status = true;
                            else
                                newObj.Status = false;
                            newObj.CreateBy = User.UserRequest() == null ? "" : User.UserRequest().Email;
                            newObj.CreateOn = (DateTime)DateTime.Now;
                            newObj.UpdateBy = User.UserRequest() == null ? "" : User.UserRequest().Email;
                            newObj.UpdateOn = (DateTime)DateTime.Now;
                            if (!string.IsNullOrWhiteSpace(Model.CreateIP))
                                newObj.CreateIP = Model.CreateIP;
                            else
                                newObj.CreateIP = "";
                            newObj.UpdateIP = newObj.CreateIP;
                            if (Model.CMSStatusFlowId.HasValue)
                                newObj.CMSStatusFlowId = Model.CMSStatusFlowId;
                            else
                                newObj.CMSStatusFlowId = 1;
                            newObj.CampaignID = Model.CampaignID;
                            newObj.CampaignName = Model.CampaignName;
                            newObj.PromotionCode = Model.PromotionCode;
                            newObj.PromotionCodeRef = Model.PromotionCodeRef;
                            newObj.MarketingAbsorb = Model.MarketingAbsorb;
                            newObj.MerchandiseAbsorb = Model.MerchandiseAbsorb;
                            newObj.VendorAbsorb = Model.VendorAbsorb;
                            newObj.Limit = Model.Limit;
                            newObj.LimitPerUser = Model.LimitPerUser;
                            newObj.ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                            db.Entry(newObj).State = EntityState.Modified;

                            if (db.SaveChanges() > 0) //Saved return row save successfully.
                            {
                                dbcxtransaction.Commit();
                                result = newObj.PromotionBuy1Get1ItemId;

                                callMongoModel.buyproducts = buypro;
                                callMongoModel.effectivedate = (DateTime)Model.EffectiveDate;
                                callMongoModel.expireddate = (DateTime)Model.ExpiryDate;
                                callMongoModel.limit = (int)newObj.Limit;
                                callMongoModel.limitperuser = (int)newObj.LimitPerUser;
                                callMongoModel.premiumproducts = getpro;
                                callMongoModel.status = (bool)newObj.Status;
                                callMongoModel.id = newObj.MongoId;
                                callMongoModel.promotioncode = newObj.PromotionCode;
                                //Update Call MongoDB
                                CallMongoAPI(callMongoModel, newObj.MongoId, "PUT", "premium", mongoReturn);

                                var rex = mongoReturn;
                                UpdatePremiumId(newObj.PromotionBuy1Get1ItemId, newObj.MongoId);
                            }
                            else
                            {
                                dbcxtransaction.Rollback();
                            }
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
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var buy1get1 = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == promotionBuy1Get1ItemId && c.ShopId == ShopId).FirstOrDefault();
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

                    result.ExpiryDate = buy1get1.ExpiryDate;

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
                    result.MongoId = buy1get1.MongoId;
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        #region Ontop List
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
                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var findPidBuy = db.PromotionBuy1Get1Item.Where(c => c.PIDBuy == Pid && c.ShopId == ShopId).ToList();
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
                    int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                    var findPidGet = db.PromotionBuy1Get1Item.Where(c => c.PIDGet == Pid && c.ShopId == ShopId).ToList();
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

        #region Update Data From MongoDB Function
        public bool UpdatePremiumId(int Buy1GetId, string MongoId)
        {
            bool result = false;
            try
            {
                int ShopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var newObj = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Buy1GetId && c.ShopId == ShopId).FirstOrDefault();
                if (newObj != null)
                {
                    newObj.MongoId = MongoId;
                    db.Entry(newObj).State = EntityState.Modified;
                    db.SaveChanges();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                //throw;
            }
            return result;

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

        #region Method Get Value
        // Get Brand List
        [HttpGet]
        [Route("api/Promotion/GetBrand/{categoryId}")]
        public HttpResponseMessage GetBrand(int categoryId)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = shopId == 0
                                ? (from product in db.Products
                                   join brand in db.Brands on product.BrandId equals brand.BrandId
                                   join category in db.GlobalCategories on product.GlobalCatId equals category.CategoryId
                                   where category.CategoryId == categoryId
                                   select new BrandRequest
                                   {
                                       BrandId = brand.BrandId,
                                       BrandNameEn = brand.BrandNameEn,
                                       BrandNameTh = brand.BrandNameTh
                                   })

                                : (from product in db.Products
                                   join brand in db.Brands on product.BrandId equals brand.BrandId
                                   join category in db.LocalCategories on product.LocalCatId equals category.CategoryId
                                   where category.CategoryId == categoryId
                                   select new BrandRequest
                                   {
                                       BrandId = brand.BrandId,
                                       BrandNameEn = brand.BrandNameEn,
                                       BrandNameTh = brand.BrandNameTh
                                   });


                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Brand");

                var items = query.ToList().GroupBy(g => g.BrandId).Select(s => s.First()).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, items);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetBrand");
            }

        }

        // Get Tag List
        [HttpGet]
        [Route("api/Promotion/GetAllTag")]
        public HttpResponseMessage GetAllTag()
        {
            try
            {
                int shopId = 0; //this.User.ShopRequest().ShopId;

                var query = db.ProductTags
                            .GroupBy(g => g.Tag)
                            .Select(s => s.FirstOrDefault());

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Tag");

                List<ProductTag> tags = new List<ProductTag>();

                foreach (var item in query)
                {
                    tags.Add(new ProductTag
                    {
                        Pid = item.Pid,
                        Tag = item.Tag
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, tags);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllTag");
            }

        }

        [HttpGet]
        [Route("api/Promotion/GetAllPaymentId")]
        public HttpResponseMessage GetAllPaymentId([FromUri] BaseCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = shopId == 0
                                ? (from cate in db.Payments
                                   select new PaymentIdModel
                                   {
                                       PaymentID = cate.PaymentID,
                                       PaymentNameEN = cate.PaymentNameEN,
                                       PaymentName = cate.PaymentName
                                   })
                                : (from cate in db.Payments
                                   select new PaymentIdModel
                                   {
                                       PaymentID = cate.PaymentID,
                                       PaymentNameEN = cate.PaymentNameEN,
                                       PaymentName = cate.PaymentName
                                   });

                if (condition != null && condition.SearchText != null)
                    query = query.Where(x => x.PaymentNameEN.Contains(condition.SearchText) || x.PaymentName.Contains(condition.SearchText));

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Brand");

                var items = query.Take(10).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, items);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCategory");
            }

        }

        // Get Category List
        [HttpGet]
        [Route("api/Promotion/GetAllCategory")]
        public HttpResponseMessage GetAllCategory([FromUri] BaseCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = shopId == 0
                                ? (from cate in db.GlobalCategories
                                   select new CategoryRequest
                                   {
                                       CategoryId = cate.CategoryId,
                                       NameEn = cate.NameEn,
                                       NameTh = cate.NameTh
                                   })
                                : (from cate in db.LocalCategories
                                   select new CategoryRequest
                                   {
                                       CategoryId = cate.CategoryId,
                                       NameEn = cate.NameEn,
                                       NameTh = cate.NameTh
                                   });

                if (condition != null && condition.SearchText != null)
                    query = query.Where(x => x.NameEn.Contains(condition.SearchText) || x.NameTh.Contains(condition.SearchText));

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Brand");

                var items = query.Take(10).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, items);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCategory");
            }

        }

        // Search Product
        [HttpGet]
        [Route("api/Promotion/SearchProduct")]
        public HttpResponseMessage SearchProduct([FromUri] ProductCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from product in db.Products select product;

                if (shopId != 0)
                    query = query.Where(x => x.ShopId == shopId);

                if (condition.SearchBy == Logic.SearchOption.PID)
                    query = query.Where(x => x.Pid.Equals(condition.SearchText));

                if (condition.SearchBy == Logic.SearchOption.SKU)
                    query = query.Where(x => x.Sku.Equals(condition.SearchText));

                if (condition.SearchBy == Logic.SearchOption.ProductName)
                    query = query.Where(x => x.ProductNameEn.Contains(condition.SearchText) || x.ProductNameTh.Contains(condition.SearchText));

                if (condition.CategoryId != null)
                    if (shopId == 0)
                        query = query.Where(x => x.GlobalCatId == condition.CategoryId);

                    else
                        query = query.Where(x => x.LocalCatId == condition.CategoryId);

                if (condition.BrandId != null)
                    query = query.Where(x => x.BrandId == condition.BrandId);

                if (condition.Tag != null)
                    query = query.Include(t => t.ProductTags)
                            .Where(x => condition.Tags.Contains(x.ProductTags.Select(s => s.Tag).FirstOrDefault()));

                query = query.Take(100);

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Product.");

                List<Product> products = new List<Product>();

                foreach (var p in query)
                {
                    Product item = new Product();
                    item.Pid = p.Pid;
                    item.ProductNameEn = p.ProductNameEn;
                    item.ProductNameTh = p.ProductNameTh;
                    item.FeatureImgUrl = p.FeatureImgUrl;
                    item.EffectiveDate = p.EffectiveDate;
                    item.ExpireDate = p.ExpireDate;
                    item.OriginalPrice = p.OriginalPrice;
                    item.Sku = p.Sku;
                    products.Add(item);
                }

                return Request.CreateResponse(HttpStatusCode.OK, products);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/SearchProduct");
            }

        }

        // Search Product by CMS Category
        [HttpGet]
        [Route("api/Promotion/SearchFeatureProduct")]
        public HttpResponseMessage SearchFeatureProduct([FromUri] ProductCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from product in db.Products
                            join cmsCatePro in db.CMSCategoryProductMaps
                            on product.Pid equals cmsCatePro.Pid
                            select new { product, cmsCatePro };

                if (condition.CMSCategoryIds != null && condition.CMSCategoryIds.Count > 0)
                    query = query.Where(x => condition.CMSCategoryIds.Contains(x.cmsCatePro.CMSCategoryId));

                if (!string.IsNullOrEmpty(condition.SearchText))
                    query = query.Where(x => x.product.ProductNameEn.Contains(condition.SearchText) || x.product.ProductNameTh.Contains(condition.SearchText));

                if (condition.ProductIds != null && condition.ProductIds.Count > 0)
                    query = query.Where(x => condition.ProductIds.Contains(x.product.Pid));

                query = query.Take(10);

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Product.");

                List<Product> products = new List<Product>();

                foreach (var p in query)
                {
                    Product item = new Product();
                    item.Pid = p.product.Pid;
                    item.ProductNameEn = p.product.ProductNameEn;
                    item.ProductNameTh = p.product.ProductNameTh;
                    products.Add(item);
                }

                return Request.CreateResponse(HttpStatusCode.OK, products);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/SearchProduct");
            }

        }
        #endregion
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
