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

namespace Colsp.Api.Controllers
{
    public class CMSManagementController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/CMSShopList")]
        [HttpGet]
        public IHttpActionResult GetByShop([FromUri] CMSShopRequest request)
        {
            try
            {
                var CMS = (from c in db.CMS
                           where c.ShopId == request.ShopId
                           select c
                                      ).Take(100);
                if (request == null)
                {
                    return Ok(CMS);
                }
                request.DefaultOnNull();
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
        [Route("api/CMSCollection")]
        [HttpPost]
        public HttpResponseMessage CreateCMS(CMSCollectionItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    CMSProcess cms = new CMSProcess();
                    int CMSId = cms.CreateCMSCollectionItem(model);
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

        #region Get CMSCollection


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
                        var GetCMS = db.CMS.Where(c => c.CMSId == CMSId).FirstOrDefault();
                        if (GetCMS != null)
                        {
                            response.CMSId = GetCMS.CMSId;
                            response.By = GetCMS.CreateBy;
                            response.CMSNameEN = GetCMS.CMSNameEN;
                            response.CMSNameTH = GetCMS.CMSNameTH;
                            response.CMSSortId = GetCMS.CMSSortId;
                            response.CMSTypeId = GetCMS.CMSTypeId;
                            response.EffectiveDate = GetCMS.EffectiveDate;
                            response.EffectiveTime = GetCMS.EffectiveTime;
                            response.ExpiryDate = GetCMS.ExpiryDate;
                            response.ExpiryTime = GetCMS.ExpiryTime;
                            response.IP = GetCMS.CreateIP;
                            response.LongDescriptionEN = GetCMS.LongDescriptionEN;
                            response.LongDescriptionTH = GetCMS.LongDescriptionTH;
                            response.ShopId = GetCMS.ShopId;
                            response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
                            response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
                            response.URLKey = GetCMS.URLKey;
                            response.Visibility = GetCMS.Visibility;

                            List<CollectionItemListResponse> Collection = new List<CollectionItemListResponse>();
                            var CollectionItemList = db.CMSCollectionItems.Where(c => c.CMSId == CMSId).ToList();
                            int CountItem = 0;
                            foreach (var itemCollection in CollectionItemList)
                            {
                                CollectionItemListResponse model = new CollectionItemListResponse();
                                model.PId = itemCollection.PId;
                                model.ProductBoxBadge = itemCollection.ProductBoxBadge;
                                model.Sequence = itemCollection.Sequence;
                                model.Status = itemCollection.Status;
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

        #endregion

        [Route("api/CMSUpdateCollection")]
        [HttpPost]
        public HttpResponseMessage UpdateCMS(CMSCollectionItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    CMSProcess cms = new CMSProcess();
                    int CMSId = cms.EditCMSCollectionItem(model);
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

        [Route("api/CMSSearchForAdd")]
        [HttpGet]
        public IHttpActionResult CMSSearchForAdd([FromUri]CMSSearchForAddRequest request)
        {
            try
            {

                dynamic response = string.Empty;
                switch (request.SearchType)
                {
                    case "Category":
                        var result = (from g in db.GlobalCategories
                                      where (g.NameEn.Contains(request.SearchText) || g.NameTh.Contains(request.SearchText))
                                      select g
                                        ).Take(100);
                        if (string.IsNullOrWhiteSpace(request.SearchText))
                        {
                            result = (from g in db.GlobalCategories
                                      select g
                                        ).Take(100);
                        }
                        if (request == null)
                        {
                            return Ok(result);
                        }
                        request.DefaultOnNull();
                        var total = result.Count();
                        var pagedCMS = result.Paginate(request);
                        response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                        break;
                    case "Shop":
                        var resultShop = (from g in db.Shops
                                          where (g.ShopNameEn.Contains(request.SearchText) || g.ShopNameTh.Contains(request.SearchText))
                                          select g
                                      ).Take(100);
                        if (request == null)
                        {
                            return Ok(resultShop);
                        }
                        request.DefaultOnNull();
                        var totalShop = resultShop.Count();
                        var pagedCMSShop = resultShop.Paginate(request);
                        response = PaginatedResponse.CreateResponse(pagedCMSShop, request, totalShop);
                        break;
                    case "Product":
                        var resultPro = (from g in db.Products
                                         where (g.ProductNameEn.Contains(request.SearchText) || g.ProductNameTh.Contains(request.SearchText))
                                         select g
                                        ).Take(100);
                        if (request == null)
                        {
                            return Ok(resultPro);
                        }
                        request.DefaultOnNull();
                        var totalPro = resultPro.Count();
                        var pagedCMSPro = resultPro.Paginate(request);
                        response = PaginatedResponse.CreateResponse(pagedCMSPro, request, totalPro);
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
