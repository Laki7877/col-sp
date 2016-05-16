using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.ByOneGetOneFunction;
using Colsp.Api.Helpers;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Colsp.Api.Controllers
{
    public class BuyOneGetOneController : ApiController
    {
        private ColspEntities db = new ColspEntities();
     
        //CRUD Sequence

        #region Create By 1 Get 1
        [Route("api/ProBuy1Get1/Create")]
        [HttpPost]
        public HttpResponseMessage CreateBy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    int Id = 0;

                    BuyOneGetOneProcess bg = new BuyOneGetOneProcess();
                    var B1G = db.PromotionBuy1Get1Item.Where(c => c.PIDBuy == model.PIDBuy ).FirstOrDefault();
                    if (B1G != null)
                    { return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "This Buy item Already Add Buy 1 Get 1 Promotion");  }

                    Id = bg.CreateBuy1Get1Item(model);
                    return GetBuy1Get1Item(Id);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot save data");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }
        #endregion
        
        #region Read By 1 Get 1
        [Route("api/ProBuy1Get1/List")]
        [HttpGet]
        public HttpResponseMessage ListBuy1Get1Item([FromUri] CMSMasterAllRequest request)
        {           
            try
            {                
                  var  ProBuy1Get1 = (from c in db.PromotionBuy1Get1Item
                           select c
                          );
              
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, ProBuy1Get1); 
                }

                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    ProBuy1Get1 = ProBuy1Get1.Where(c => (c.NameEN.Contains(request.SearchText) 
                      || c.NameTH.Contains(request.SearchText)  
                      || c.ShortDescriptionEN.Contains(request.SearchText) 
                      || c.ShortDescriptionTH.Contains(request.SearchText)
                      || c.LongDescriptionEN.Contains(request.SearchText)
                      || c.ProductBoxBadge.Contains(request.SearchText)
                    ));
                }

                if (!string.IsNullOrEmpty(request._filter))
                {                    
                    //if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_DRAFT );
                    //}
                    //else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_APPROVE);
                    //}
                    //else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_NOT_APPROVE);
                    //}
                    //else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId ==  Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
                    //}
                }

                var total = ProBuy1Get1.Count();
                var pagedProBuy1Get1 = ProBuy1Get1.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProBuy1Get1, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, request);            
            }

        }

        [Route("api/ProBuy1Get1/{Id}")]
        [HttpGet]
        public HttpResponseMessage GetBuy1Get1Item(int? Id)
        {
            try
            {
                Buy1Get1ItemResponse response = new Buy1Get1ItemResponse();
                if (Id != null && Id.HasValue)
                {
                    if (Id == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Item id is invalid. Cannot find item in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var B1G = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Id).FirstOrDefault();
                        var tmpPnameBuy = db.ProductStages.Where( p => p.ProductId == B1G.PIDBuy).SingleOrDefault() != null ? db.ProductStages.Where(p => p.ProductId == B1G.PIDBuy).SingleOrDefault().ProductNameEn : "" ;
                        var tmpPnameGet = db.ProductStages.Where(p => p.ProductId == B1G.PIDGet).SingleOrDefault() != null ? db.ProductStages.Where(p => p.ProductId == B1G.PIDGet).SingleOrDefault().ProductNameEn : "";

                        if (B1G != null)
                        {
                            response.PromotionBuy1Get1ItemId = B1G.PromotionBuy1Get1ItemId;
                            response.NameEN = B1G.NameEN;
                            response.NameTH = B1G.NameTH;
                            response.URLKey = B1G.URLKey;
                            response.PIDBuy = B1G.PIDBuy;
                            response.PIDGet = B1G.PIDGet;
                            response.ShortDescriptionTH = B1G.ShortDescriptionTH;
                            response.LongDescriptionTH = B1G.LongDescriptionTH;
                            response.ShortDescriptionEN = B1G.ShortDescriptionEN;
                            response.LongDescriptionEN = B1G.LongDescriptionEN;
                            response.EffectiveDate = B1G.EffectiveDate;
                            response.EffectiveTime = B1G.EffectiveTime;
                            response.ExpiryDate = B1G.ExpiryDate;
                            response.ExpiryTime = B1G.ExpiryTime;
                            response.ProductBoxBadge = B1G.ProductBoxBadge;
                            response.Sequence = B1G.Sequence;
                            response.Status = B1G.Status;
                            response.CreateBy = B1G.CreateBy;
                            response.Createdate = (DateTime)B1G.Createdate;
                            response.UpdateBy = B1G.UpdateBy;
                            response.UpdateDate = (DateTime)B1G.UpdateDate;
                            response.CreateIP = B1G.CreateIP;
                            response.UpdateIP = B1G.UpdateIP;

                            response.CMSStatusFlowId = B1G.CMSStatusFlowId;
                            //response.Visibility = B1G.Visibility;
                            response.PNameBuy = tmpPnameBuy;
                            response.PNameGet = tmpPnameGet;

                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find item in System");
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        
        #endregion

        #region Update & Delete By 1 Get 1
        //single object input
        [Route("api/ProBuy1Get1/UpdateItem")]
        [HttpPost]
        public HttpResponseMessage UpdateProBuy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {                   
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.UpdateProBuy1Get1(model);                   
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Update Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        [Route("api/ProBuy1Get1/EditItem")]
        [HttpPost]
        public HttpResponseMessage EditProBuy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {                   
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.EditProBuy1Get1(model);                   
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Edit Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        //listing object input
        [Route("api/ProBuy1Get1/UpdateList")]
        [HttpPost]
        public HttpResponseMessage UpdateProBuy1Get1List(List<Buy1Get1ItemRequest> model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        var B1G = db.PromotionBuy1Get1Item.Where(c => c.PIDBuy == item.PIDBuy).FirstOrDefault();
                        if (B1G != null && B1G.PromotionBuy1Get1ItemId != item.PromotionBuy1Get1ItemId)
                        { return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "This Buy item Already Add Buy 1 Get 1 Promotion"); }
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.UpdateProBuy1Get1(item);                       
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return GetBuy1Get1Item(Id);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        [Route("api/ProBuy1Get1/EditList")]
        [HttpPost]
        public HttpResponseMessage EditProBuy1Get1List(List<Buy1Get1ItemRequest> model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.EditProBuy1Get1(item);
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Edit Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }
        #endregion

        #region Duplicate & Import , Export

        [Route("api/ProBuy1Get1/Export")]
        [HttpPost]
        public HttpResponseMessage ExportBuyOneGetOne(List<Buy1Get1ItemRequest> request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                string header = @"NameEN,NameTH,URLKey,Buy ItemName,GetItemName,EffectiveDate,ExpiryDate,ShortDescriptionTH,ShortDescriptionEN,LongDescriptionTH,LongDescriptionEN,Status,Visible";
                writer.WriteLine(header);
                StringBuilder sb = null;
                foreach (Buy1Get1ItemRequest rq in request)
                {
                    sb = new StringBuilder();
                    if (rq.PromotionBuy1Get1ItemId == default(int)) { throw new Exception("Buy 1 Get 1 Id cannot be null"); }
                    var coll = db.PromotionBuy1Get1Item.Find(rq.PromotionBuy1Get1ItemId);
                    //var cmsFlowStatus = db.CMSMasterStatus.Find(coll.CMSStatusFlowId);
                    var buyItemName = db.Products.Find(coll.PIDBuy);
                    var getItemName = db.Products.Find(coll.PIDGet);
                    //var visible = coll.Visibility != null ? (coll.Visibility == true ? "Visible" : "InVisible") : "unknow";
                    if (coll == null)
                    {
                        throw new Exception("Cannot find Collection with id " + rq.PromotionBuy1Get1ItemId);
                    }
                    
                    //sb.Append(coll.PromotionBuy1Get1ItemId); sb.Append(",");
                    sb.Append(coll.NameEN); sb.Append(",");
                    sb.Append(coll.NameTH); sb.Append(",");
                    sb.Append(coll.URLKey); sb.Append(",");
                    sb.Append(buyItemName); sb.Append(",");
                    sb.Append(getItemName); sb.Append(",");
                                     
                    sb.Append(coll.EffectiveDate); sb.Append(",");
                    //sb.Append(coll.EffectiveTime); sb.Append(",");
                    sb.Append(coll.ExpiryDate); sb.Append(",");
                    //sb.Append(coll.ExpiryTime); sb.Append(",");
                    //sb.Append(coll.ProductBoxBadge); sb.Append(",");
                    //sb.Append(coll.Sequence); sb.Append(",");
                    //sb.Append(coll.Status); sb.Append(",");
                    //sb.Append(coll.CreateBy); sb.Append(",");
                    //sb.Append(coll.Createdate); sb.Append(",");
                    //sb.Append(coll.UpdateBy); sb.Append(",");
                    //sb.Append(coll.UpdateDate); sb.Append(",");
                    //sb.Append(coll.CreateIP); sb.Append(",");
                    //sb.Append(coll.UpdateIP); sb.Append(",");
                    //sb.Append(coll.CMSStatusFlowId); sb.Append(",");
                    //sb.Append(coll.Visibility); sb.Append(",");
                    sb.Append(coll.ShortDescriptionTH); sb.Append(",");
                    sb.Append(coll.ShortDescriptionEN); sb.Append(",");

                    if (!string.IsNullOrEmpty(coll.LongDescriptionTH))
                    {
                        if (coll.LongDescriptionTH.Contains("\""))
                        {
                            coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH.Replace("\"", "\"\""));
                        }
                        if (coll.LongDescriptionTH.Contains(","))
                        {
                            coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
                        }
                        if (coll.LongDescriptionTH.Contains(System.Environment.NewLine))
                        {
                            coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
                        }
                    }
                    if (!string.IsNullOrEmpty(coll.LongDescriptionEN))
                    {
                        if (coll.LongDescriptionEN.Contains("\""))
                        {
                            coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN.Replace("\"", "\"\""));
                        }
                        if (coll.LongDescriptionEN.Contains(","))
                        {
                            coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
                        }
                        if (coll.LongDescriptionEN.Contains(System.Environment.NewLine))
                        {
                            coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
                        }
                    }
                    sb.Append("\"" + coll.LongDescriptionTH + "\""); sb.Append(",");
                    sb.Append("\"" + coll.LongDescriptionEN + "\""); sb.Append(",");
                    //sb.Append(cmsFlowStatus.CMSMasterStatusNameEN); sb.Append(",");
                    //sb.Append(visible); sb.Append(",");


                    writer.WriteLine(sb);
                }
                writer.Flush();
                stream.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                return result;
            }
            catch (Exception e)
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e);
            }
        }


        #endregion
    }
}