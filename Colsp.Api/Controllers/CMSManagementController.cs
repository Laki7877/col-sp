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
using Colsp.Api.CMSFunction;
using System.IO;
using System.Text;
using System.Net.Http.Headers;

namespace Colsp.Api.Controllers
{
    public class CMSManagementController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        #region GetShopId
        private int GetShopId()
        {
            var shopId = this.User.Shops().FirstOrDefault();
            return 0;
        }
        #endregion

        #region Page List
        [Route("api/CMSShopList")]
        [HttpGet]
        public IHttpActionResult GetByShop([FromUri] CMSShopRequest request)
        {
            int? shopId = (int?)this.User.ShopRequest().ShopId.Value;
            try
            {
                if (!shopId.HasValue)
                    return Ok(request);
                IQueryable<CMSMaster> CMS;
                if (shopId.HasValue && shopId.Equals(Constant.CMS_SHOP_GOBAL))
                {
                    CMS = (from c in db.CMSMasters
                           select c
                          ).Take(100);
                }
                else
                {
                    CMS = (from c in db.CMSMasters
                           where c.ShopId == shopId
                           select c
                          ).Take(100);
                }
                if (request == null)
                {
                    return Ok(CMS);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    CMS = CMS.Where(c => (c.CMSNameEN.Contains(request.SearchText) || c.CMSNameTH.Contains(request.SearchText)));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_DRAFT);
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_APPROVE);
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_NOT_APPROVE);
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
                    }
                }
                var total = CMS.Count();
                var pagedCMS = CMS.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(request);
            }

        }

        [Route("api/CMSUpdateStatus")]
        [HttpPut]
        public IHttpActionResult UpdateCMSStatusFormCMSList(UpdateCMSStatusRequest model)
        {
            CMSShopRequest CMSResult = new CMSShopRequest();
            CMSResult.SearchText = model.SearchText;
            //CMSResult.ShopId = model.ShopId;
            CMSResult._direction = model._direction;
            CMSResult._filter = model._filter;
            CMSResult._limit = model._limit;
            CMSResult._offset = model._offset;
            CMSResult._order = model._order;
            try
            {
                if (model != null)
                {
                    if (model.ShopId.HasValue)
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSResult = cms.CMSUpdateStatus(model);
                        return GetCMSList(CMSResult);
                    }
                }
                return Ok(CMSResult);
            }
            catch (Exception ex)
            {
                return Ok(CMSResult);
            }
        }
        #endregion


        [Route("api/CMSStages")]
        [HttpPost]
        public HttpResponseMessage CreateCMS(CMSCollectionItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    int CMSId = 0;
                    if (model.CMSTypeId.Equals(Constant.CMS_TYPE_STATIC_PAGE))
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSId = cms.CreateCMSStaticPage(model);
                    }
                    else if (model.CMSTypeId.Equals(Constant.CMS_TYPE_COLLECTION_PAGE))
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSId = cms.CreateCMSCollectionItem(model);
                    }
                    return GetCollection(CMSId);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }



        #region Get CMS List

        [Route("api/CMSStages/{CMSId}")]
        [HttpGet]
        public HttpResponseMessage GetCollection(int? CMSId)
        {
            try
            {
                CMSCollectionResponse response = new CMSCollectionResponse();
                if (CMSId != null && CMSId.HasValue)
                {
                    if (CMSId == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CMS ID is invalid. Cannot find CMSId in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var GetCMS = db.CMSMasters.Where(c => c.CMSId == CMSId).FirstOrDefault();
                        if (GetCMS != null)
                        {
                            response.CMSId = GetCMS.CMSId;
                            response.CreateBy = GetCMS.CreateBy;
                            response.CMSNameEN = GetCMS.CMSNameEN;
                            response.CMSNameTH = GetCMS.CMSNameTH;
                            response.CMSFilterId = GetCMS.CMSFilterId;
                            response.CMSTypeId = GetCMS.CMSTypeId;
                            response.EffectiveDate = GetCMS.EffectiveDate;
                            response.EffectiveTime = GetCMS.EffectiveTime;
                            response.ExpiryDate = GetCMS.ExpiryDate;
                            response.ExpiryTime = GetCMS.ExpiryTime;
                            response.CreateIP = GetCMS.CreateIP;
                            response.LongDescriptionEN = GetCMS.LongDescriptionEN;
                            response.LongDescriptionTH = GetCMS.LongDescriptionTH;
                            response.ShopId = GetCMS.ShopId;
                            response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
                            response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
                            response.CMSStatusFlowId = GetCMS.CMSStatusFlowId;
                            response.URLKey = GetCMS.URLKey;
                            response.Visibility = GetCMS.Visibility;
                            response.CreateDate = (DateTime)GetCMS.Createdate;
                            response.CMSCollectionGroupId = GetCMS.CMSCollectionGroupId;
                            List<CollectionItemListResponse> Collection = new List<CollectionItemListResponse>();
                            var CollectionItemList = db.CMSCollectionItems.Where(c => c.CMSId == CMSId).ToList();
                            int CountItem = 0;
                            foreach (var itemCollection in CollectionItemList)
                            {
                                CollectionItemListResponse model = new CollectionItemListResponse();
                                model.CMSId = GetCMS.CMSId;
                                model.PId = itemCollection.PId;
                                model.ProductBoxBadge = itemCollection.ProductBoxBadge;
                                model.Sequence = itemCollection.Sequence;
                                model.Status = itemCollection.Status;
                                model.CMSCollectionItemGroupId = itemCollection.CMSCollectionItemGroupId;
                                Collection.Add(model);
                                CountItem++;
                            }
                            response.CollectionItemList = Collection;
                            response.CMSCount = CountItem;
                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find CMS ID in System");
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

        public IHttpActionResult GetCMSList(CMSShopRequest request)
        {
            int? shopId = (int?)this.User.ShopRequest().ShopId.Value;
            try
            {
                if (!shopId.HasValue)
                    return Ok(request);
                IQueryable<CMSMaster> CMS;
                if (shopId.HasValue && shopId.Equals(Constant.CMS_SHOP_GOBAL))
                {
                    CMS = (from c in db.CMSMasters
                           select c
                          ).Take(100);
                }
                else
                {
                    CMS = (from c in db.CMSMasters
                           where c.ShopId == shopId
                           select c
                          ).Take(100);
                }
                if (request == null)
                {
                    return Ok(CMS);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    CMS = CMS.Where(c => (c.CMSNameEN.Contains(request.SearchText) || c.CMSNameTH.Contains(request.SearchText)));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        CMS = CMS.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_WAIT_FOR_APPROVAL));
                    }
                }
                var total = CMS.Count();
                var pagedCMS = CMS.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(request);
            }

        }


        #endregion

        #region Search
        [Route("api/CMSSearchForAdd")]
        [HttpGet]
        public IHttpActionResult CMSSearchForAdd([FromUri]CMSSearchForAddRequest request)
        {
            var UserType = this.User.UserRequest().Type;
            int? shopId = 0;
            switch (UserType)
            {
                case "A":
                    shopId = 0;
                    break;
                case "S":
                case "H":
                    try
                    {
                        shopId = this.User.ShopRequest().ShopId.Value;
                    }
                    catch (Exception ex)
                    {
                        return Ok(request);
                        throw;
                    }

                    break;
                default:
                    shopId = null;
                    break;
            }
            try
            {
                if (!shopId.HasValue)
                    return Ok(request);
                dynamic response = string.Empty;
                switch (request.SearchType.ToUpper())
                {
                    case "CATEGORY":
                        if (string.IsNullOrWhiteSpace(request._order))
                            request._order = "CategoryId";
                        if (!shopId.Equals(Constant.CMS_SHOP_GOBAL)) //Local
                        {
                            var result = (from g in db.LocalCategories
                                          where g.ShopId == shopId
                                          select new
                                          {
                                              g.CategoryId,
                                              g.NameEn,
                                              g.NameTh,
                                              g.Shop.ShopNameEn,
                                              g.Shop.ShopNameTh,
                                              g.Status,
                                              g.Visibility
                                          }
                                       ).Take(100);

                            if (request == null)
                            {
                                return Ok(result);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                result = result.Where(c => (c.NameEn.Contains(request.SearchText) || c.NameTh.Contains(request.SearchText)));
                            }
                            var total = result.Count();
                            var pagedCMS = result.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                        }
                        else //Global
                        {
                            var result = (from g in db.GlobalCategories
                                          select new
                                          {
                                              g.CategoryId,
                                              g.NameEn,
                                              g.NameTh,
                                              g.Status,
                                              g.Visibility
                                          }
                                      ).Take(100);

                            if (request == null)
                            {
                                return Ok(result);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                result = result.Where(c => (c.NameEn.Contains(request.SearchText) || c.NameTh.Contains(request.SearchText)));
                            }
                            var total = result.Count();
                            var pagedCMS = result.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                        }



                        break;
                    case "BRAND":
                        if (string.IsNullOrWhiteSpace(request._order))
                            request._order = "BrandId";
                        if (!shopId.Equals(Constant.CMS_SHOP_GOBAL)) //Local
                        {

                            var resultBrand = (from g in db.Brands
                                               join p in db.Products on g.BrandId equals p.BrandId
                                               where p.ShopId == shopId
                                               select new
                                               {
                                                   g.BrandId,
                                                   g.BrandNameEn,
                                                   g.BrandNameTh,
                                                   g.Status

                                               }
                                        ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultBrand);
                            }
                            request.DefaultOnNull();
                            var totalBrand = resultBrand.Count();
                            var pagedCMSBrand = resultBrand.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSBrand, request, totalBrand);
                        }
                        else
                        {
                            var resultBrand = (from g in db.Brands
                                               where (g.BrandNameEn.Contains(request.SearchText) || g.BrandNameTh.Contains(request.SearchText))
                                               select new
                                               {
                                                   g.BrandId,
                                                   g.BrandNameEn,
                                                   g.BrandNameTh,
                                                   g.Status

                                               }
                                        ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultBrand);
                            }
                            request.DefaultOnNull();
                            var totalBrand = resultBrand.Count();
                            var pagedCMSBrand = resultBrand.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSBrand, request, totalBrand);
                        }
                        break;
                    case "SHOP":
                        if (string.IsNullOrWhiteSpace(request._order))
                            request._order = "ShopId";
                        if (!shopId.Equals(Constant.CMS_SHOP_GOBAL)) //Local
                        {
                            var resultShop = (from g in db.Shops
                                              where g.ShopId == shopId
                                              select new
                                              {
                                                  g.ShopId,
                                                  g.ShopNameEn,
                                                  g.ShopNameTh,
                                                  g.Status

                                              }
                                      ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultShop);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                resultShop = resultShop.Where(c => (c.ShopNameEn.Contains(request.SearchText) || c.ShopNameTh.Contains(request.SearchText)));
                            }
                            var totalShop = resultShop.Count();
                            var pagedCMSShop = resultShop.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSShop, request, totalShop);
                        }
                        else
                        {
                            var resultShop = (from g in db.Shops
                                              select new
                                              {
                                                  g.ShopId,
                                                  g.ShopNameEn,
                                                  g.ShopNameTh,
                                                  g.Status

                                              }
                                      ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultShop);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                resultShop = resultShop.Where(c => (c.ShopNameEn.Contains(request.SearchText) || c.ShopNameTh.Contains(request.SearchText)));
                            }
                            var totalShop = resultShop.Count();
                            var pagedCMSShop = resultShop.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSShop, request, totalShop);
                        }

                        break;
                    case "PRODUCT":
                        if (string.IsNullOrWhiteSpace(request._order))
                            request._order = "ProductId";
                        if (!shopId.Equals(Constant.CMS_SHOP_GOBAL)) //Local
                        {
                            var resultPro = (from g in db.Products
                                             where g.ShopId == shopId
                                             select new
                                             {
                                                 g.GlobalCatId,
                                                 g.BrandId,
                                                 g.ProductId,
                                                 g.ShopId,
                                                 g.Pid,
                                                 g.ProductNameEn,
                                                 g.ProductNameTh,
                                                 g.Status

                                             }
                                           ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultPro);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                resultPro = resultPro.Where(c => (c.ProductNameEn.Contains(request.SearchText) || c.ProductNameTh.Contains(request.SearchText)));
                            }
                            var totalPro = resultPro.Count();
                            var pagedCMSPro = resultPro.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSPro, request, totalPro);
                        }
                        else {
                            var resultPro = (from g in db.Products
                                             select new
                                             {
                                                 g.GlobalCatId,
                                                 g.BrandId,
                                                 g.ProductId,
                                                 g.ShopId,
                                                 g.Pid,
                                                 g.ProductNameEn,
                                                 g.ProductNameTh,
                                                 g.Status

                                             }
                                           ).Take(100);
                            if (request == null)
                            {
                                return Ok(resultPro);
                            }
                            request.DefaultOnNull();
                            if (!string.IsNullOrEmpty(request.SearchText))
                            {
                                resultPro = resultPro.Where(c => (c.ProductNameEn.Contains(request.SearchText) || c.ProductNameTh.Contains(request.SearchText)));
                            }
                            var totalPro = resultPro.Count();
                            var pagedCMSPro = resultPro.Paginate(request);
                            response = PaginatedResponse.CreateResponse(pagedCMSPro, request, totalPro);
                        }

                        break;
                    default:
                        break;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(request);
            }
        }

        #endregion


        #region Edit/Update CMS
        [Route("api/CMSEditStages")]
        [HttpPost]
        public HttpResponseMessage EditCMS(CMSCollectionItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    int CMSId = 0;
                    if (model.CMSTypeId.Equals(Constant.CMS_TYPE_STATIC_PAGE))
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSId = cms.EditCMSStaticPage(model);
                    }
                    else if (model.CMSTypeId.Equals(Constant.CMS_TYPE_COLLECTION_PAGE))
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSId = cms.EditCMSCollectionItem(model);
                    }
                    return GetCollection(CMSId);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }

        //Thanakrit : 20160215 , add for minified transfer json obj from front end
        //used it's own data if not transfer
        [Route("api/CMSUpdateStages")]
        [HttpPost]
        public HttpResponseMessage UpdateCMS(List<CMSCollectionItemRequest> model)
        {
            try
            {
                int CMSId = 0;
                if (model != null)
                {

                    foreach (var item in model)
                    {

                        if (item.CMSTypeId.Equals(Constant.CMS_TYPE_STATIC_PAGE))
                        {
                            CMSProcess cms = new CMSProcess();
                            CMSId = cms.UpdateCMSStaticPage(item);
                        }
                        else if (item.CMSTypeId.Equals(Constant.CMS_STATUS_WAIT_FOR_APPROVAL))

                        {
                            CMSProcess cms = new CMSProcess();
                            CMSId = cms.UpdateCMSCollectionItem(item);
                        }

                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return GetCollection(CMSId);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }


        }
        #endregion

        #region export

        [Route("api/CMSStages/Export")]
        [HttpPost]
        public HttpResponseMessage ExportCollection(List<CMSCollectionItemRequest> request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                string header = @"Collection ID,Collection English Name,Collection Thai Name,URL Keyword,EffectiveDate,EffectiveTime,ExpiryDate,ExpiryTime,ShopId,CMSCount,ShortDescriptionTH,ShortDescriptionEN,LongDescriptionTH,LongDescriptionEN,Sequence,Collection Status,Visibility";
                writer.WriteLine(header);
                StringBuilder sb = null;
                foreach (CMSCollectionItemRequest rq in request)
                {
                    sb = new StringBuilder();
                    if (rq.CMSId == default(int)) { throw new Exception("Collection Id cannot be null"); }
                    var coll = db.CMSMasters.Find(rq.CMSId);
                    var cmsFlowStatus = db.CMSStatusFlows.Find(coll.CMSStatusFlowId);
                    var visible = coll.Visibility != null ? (coll.Visibility == true ? "Visible" : "InVisible") : "unknow";
                    if (coll == null)
                    {
                        throw new Exception("Cannot find Collection with id " + rq.CMSId);
                    }
                    sb.Append(coll.CMSId); sb.Append(",");
                    sb.Append(coll.CMSNameEN); sb.Append(",");
                    sb.Append(coll.CMSNameTH); sb.Append(",");
                    sb.Append(coll.URLKey); sb.Append(",");
                    //sb.Append(coll.CMSTypeId); sb.Append(",");
                    //sb.Append(coll.CMSFilterId); sb.Append(",");
                    sb.Append(coll.EffectiveDate); sb.Append(",");
                    sb.Append(coll.EffectiveTime); sb.Append(",");
                    sb.Append(coll.ExpiryDate); sb.Append(",");
                    sb.Append(coll.ExpiryTime); sb.Append(",");
                    sb.Append(coll.ShopId); sb.Append(",");
                    sb.Append(coll.CMSCount); sb.Append(",");
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
                    sb.Append(coll.Sequence); sb.Append(",");
                    //sb.Append(coll.CMSCollectionGroupId); sb.Append(",");
                    sb.Append(cmsFlowStatus.CMSStatusName); sb.Append(",");
                    sb.Append(visible); sb.Append(",");
                    //sb.Append(coll.CreateBy); sb.Append(",");
                    //sb.Append(coll.Createdate); sb.Append(",");
                    //sb.Append(coll.UpdateBy); sb.Append(",");
                    //sb.Append(coll.UpdateDate); sb.Append(",");
                    //sb.Append(coll.CreateIP); sb.Append(",");
                    //sb.Append(coll.UpdateIP); sb.Append(",");


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
