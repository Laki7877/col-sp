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
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using Colsp.Api.CMSFunction;

namespace Colsp.Api.Controllers
{
    public class CMSManagementController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        #region GetShopId
        private int GetShopId()
        {
            var ShopId = this.User.ShopRequest().ShopId;
            return 0;
        }
        #endregion

        #region Page List Get All CMS Master
        [Route("api/CMS/GetAll")]
        [HttpGet]
        public IHttpActionResult GetCMSALL([FromUri] CMSMasterAllRequest request)
        {
            try
            {
                IQueryable<CMSMaster> CMS;
                dynamic response = string.Empty;
                if (this.User.UserRequest().Type == Constant.USER_TYPE_ADMIN)
                {
                    CMS = (from c in db.CMSMasters
                           select c
                               ).Take(100);
                    if (request == null)
                    {
                        return Ok(CMS);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrEmpty(request.SearchText))
                    {
                        CMS = CMS.Where(c => (c.CMSMasterNameEN.Contains(request.SearchText) || c.CMSMasterNameTH.Contains(request.SearchText)));
                    }
                    if (!string.IsNullOrEmpty(request._filter))
                    {

                        if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_DRAFT);
                        }
                        else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_APPROVE);
                        }
                        else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_NOT_APPROVE);
                        }
                        else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
                        }
                    }
                    var total = CMS.Count();
                    var pagedCMS = CMS.Paginate(request);
                    response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                }
                else
                {
                    int? shopId = (int?)this.User.ShopRequest().ShopId;

                    if (!shopId.HasValue)
                        return Ok(request);

                    CMS = (from c in db.CMSMasterGroupMaps
                           join m in db.CMSMasters on c.CMSMasterId equals m.CMSMasterId
                           where c.ShopId == shopId
                           select m
                          ).Take(100);

                    if (request == null)
                    {
                        return Ok(CMS);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrEmpty(request.SearchText))
                    {
                        CMS = CMS.Where(c => (c.CMSMasterNameEN.Contains(request.SearchText) || c.CMSMasterNameTH.Contains(request.SearchText)));
                    }
                    if (!string.IsNullOrEmpty(request._filter))
                    {

                        if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_DRAFT);
                        }
                        else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_APPROVE);
                        }
                        else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_NOT_APPROVE);
                        }
                        else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                        {
                            CMS = CMS.Where(p => p.CMSMasterStatusId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
                        }
                    }
                    var total = CMS.Count();
                    var pagedCMS = CMS.Paginate(request);
                    response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                }


                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(request);
            }

        }

        [Route("api/CMS/UpdateItemList")]
        [HttpPut]
        public IHttpActionResult UpdateCMSStatusFormCMSList(CMSMasterItemListRequest model)
        {
            CMSMasterAllRequest CMSResult = new CMSMasterAllRequest();
            CMSResult.SearchText = model.SearchText;
            CMSResult._direction = model._direction;
            CMSResult._filter = model._filter;
            CMSResult._limit = model._limit;
            CMSResult._offset = model._offset;
            CMSResult._order = model._order;
            try
            {
                if (model != null)
                {
                    int? ShopId = this.User.ShopRequest().ShopId;
                    int UserId = this.User.UserRequest().UserId;
                    if (ShopId.HasValue)
                    {
                        CMSProcess cms = new CMSProcess();
                        CMSResult = cms.CMSUpdateStatus(model, UserId, ShopId);
                        return GetCMSALL(CMSResult);
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


        //[Route("api/CMSStages")]
        //[HttpPost]
        //public HttpResponseMessage CreateCMS(CMSCollectionItemRequest model)
        //{
        //    try
        //    {
        //        if (model != null)
        //        {
        //            int CMSId = 0;
        //            if (model.CMSTypeId.Equals(Constant.CMS_TYPE_STATIC_PAGE))
        //            {
        //                CMSProcess cms = new CMSProcess();
        //                CMSId = cms.CreateCMSStaticPage(model);
        //            }
        //            else if (model.CMSTypeId.Equals(Constant.CMS_TYPE_COLLECTION_PAGE))
        //            {
        //                CMSProcess cms = new CMSProcess();
        //                CMSId = cms.CreateCMSCollectionItem(model);
        //            }
        //            return GetCMSMasterDetail(CMSId);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}

        //[Route("api/CMSGroup")]
        //[HttpPost]
        //public HttpResponseMessage CMSGroupCreate(CMSGroupRequest model)
        //{
        //    try
        //    {
        //        if (model != null)
        //        {
        //            int CMSGroupId = 0;
        //            CMSProcess cmsGroup = new CMSProcess();
        //            CMSGroupId = cmsGroup.CreateCMSGroup(model);
        //            return GetCMSGroup(CMSGroupId);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}


        #region Get CMS List
        //[Route("api/CMSGroup/{CMSGroupId}")]
        //[HttpPost]
        //public HttpResponseMessage GetCMSGroup(int? CMSGroupId)
        //{
        //    try
        //    {
        //        CMSGroupResponse response = new CMSGroupResponse();
        //        if (CMSGroupId != null && CMSGroupId.HasValue)
        //        {
        //            if (CMSGroupId == 0)
        //                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CMS ID is invalid. Cannot find CMSId in System");
        //            using (ColspEntities db = new ColspEntities())
        //            {
        //                var GetCMS = db.CMSGroups.Where(c => c.CMSGroupId == CMSGroupId).FirstOrDefault();
        //                if (GetCMS != null)
        //                {
        //                    response.CMSGroupId = GetCMS.CMSGroupId;
        //                    response.CreateBy = GetCMS.CreateBy;
        //                    response.CMSGroupNameEN = GetCMS.CMSGroupNameEN;
        //                    response.CMSGroupNameTH = GetCMS.CMSGroupNameTH;
        //                    response.Sequence = (int)GetCMS.Sequence;
        //                    List<CMSMasterResponse> MasterList = new List<CMSMasterResponse>();
        //                    var GetCMSMasterIdList = db.CMSMasterGroupMaps.Where(c => c.CMSMasterGroupId == GetCMS.CMSGroupId).Select(c => c.CMSMasterId).ToList();
        //                    foreach (int CMSMasteritem in GetCMSMasterIdList)
        //                    {
        //                        CMSMasterResponse CMSMaster = new CMSMasterResponse();
        //                        CMSMaster = this.GetCMSMasterDetailById(CMSMasteritem);
        //                        MasterList.Add(CMSMaster);
        //                    }
        //                    response.CMSMasterList = MasterList;

        //                    return Request.CreateResponse(HttpStatusCode.OK, response);
        //                }
        //                else
        //                {
        //                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find CMS ID in System");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[Route("api/CMSMaster/{CMSId}")]
        //[HttpGet]
        //public HttpResponseMessage GetCMSMasterDetail(int? CMSId)
        //{
        //    try
        //    {
        //        CMSMasterResponse response = new CMSMasterResponse();
        //        if (CMSId != null && CMSId.HasValue)
        //        {
        //            if (CMSId == 0)
        //                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CMS ID is invalid. Cannot find CMSId in System");
        //            using (ColspEntities db = new ColspEntities())
        //            {
        //                var GetCMS = db.CMSMasters.Where(c => c.CMSMasterId == CMSId).FirstOrDefault();
        //                if (GetCMS != null)
        //                {
        //                    response.CMSMasterId = GetCMS.CMSMasterId;
        //                    string CreateBy = string.Empty;
        //                    if (GetCMS.CreateBy.HasValue)
        //                    {
        //                        var by = db.Users.Where(c => c.UserId == GetCMS.CreateBy).FirstOrDefault();
        //                        if (by != null)
        //                            CreateBy = by.NameEn;
        //                    }
        //                    response.CreateBy = CreateBy;
        //                    response.CMSMasterNameEN = GetCMS.CMSMasterNameEN;
        //                    response.CMSMasterNameTH = GetCMS.CMSMasterNameTH;
        //                    string CMSType = string.Empty;
        //                    if (GetCMS.CreateBy.HasValue)
        //                    {
        //                        var Type = db.CMSMasterTypes.Where(c => c.CMSMasterTypeId == GetCMS.CMSTypeId).FirstOrDefault();
        //                        if (Type != null)
        //                            CMSType = Type.CMSMasterTypeNameEN;
        //                    }
        //                    response.CMSType = CMSType;
        //                    response.CMSMasterEffectiveDate = GetCMS.CMSMasterEffectiveDate;
        //                    response.CMSMasterEffectiveTime = GetCMS.CMSMasterEffectiveTime;
        //                    response.CMSMasterExpiryDate = GetCMS.CMSMasterExpiryDate;
        //                    response.CMSMasterExpiryTime = GetCMS.CMSMasterExpiryTime;
        //                    response.CreateIP = GetCMS.CreateIP;
        //                    response.LongDescriptionEN = GetCMS.LongDescriptionEN;
        //                    response.LongDescriptionTH = GetCMS.LongDescriptionTH;
        //                    response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
        //                    response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
        //                    response.CMSMasterStatusId = GetCMS.CMSMasterStatusId;
        //                    string Status = string.Empty;
        //                    if (GetCMS.CMSMasterStatusId.HasValue)
        //                    {
        //                        var st = db.CMSMasterStatus.Where(c => c.CMSMasterStatusId == GetCMS.CMSMasterStatusId).FirstOrDefault();
        //                        if (st != null)
        //                            Status = st.CMSMasterStatusNameEN;
        //                    }
        //                    response.CMSStatus = Status;
        //                    response.CMSMasterURLKey = GetCMS.CMSMasterURLKey;
        //                    response.Visibility = GetCMS.Visibility;
        //                    response.CreateDate = (DateTime)GetCMS.Createdate;

        //                    List<CategoryListResponse> CategoryList = new List<CategoryListResponse>();
        //                    var CategoryLists = (from map in db.CMSMastserCategoryMaps.Where(m => m.CMSMasterId == CMSId)
        //                                         from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
        //                                         select new
        //                                         {
        //                                             CMSMasterId = map.CMSMasterId,
        //                                             CMSCategoryId = map.CMSCategoryId,
        //                                             CategoryNameEN = cat.CMSCategoryNameEN,
        //                                             CategoryNameTH = cat.CMSCategoryNameTH
        //                                         });
        //                    int CountItem = 0;
        //                    foreach (var itemCat in CategoryLists)
        //                    {
        //                        CategoryListResponse model = new CategoryListResponse();
        //                        model.CMSMasterId = GetCMS.CMSMasterId;
        //                        model.CMSCategoryId = itemCat.CMSCategoryId;
        //                        List<ProductListResponse> ProductList = new List<ProductListResponse>();
        //                        var ProductLists = (from map in db.CMSCategoryProductMaps.Where(m => m.CMSCategoryId == itemCat.CMSCategoryId)
        //                                            from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
        //                                            from pro in db.Products.Where(c => c.Pid == map.ProductPID).DefaultIfEmpty()
        //                                            select new ProductListResponse
        //                                            {
        //                                                CMSCategoryId = (int)map.CMSCategoryId,
        //                                                ProductNameEN = pro.ProductNameEn,
        //                                                ProductNameTH = pro.ProductNameTh
        //                                            });
        //                        CategoryList.Add(model);
        //                        CountItem++;
        //                    }
        //                    response.CategoryLists = CategoryList;
        //                    return Request.CreateResponse(HttpStatusCode.OK, response);
        //                }
        //                else
        //                {
        //                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find CMS ID in System");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //public CMSMasterResponse GetCMSMasterDetailById(int? CMSId)
        //{
        //    CMSMasterResponse response = new CMSMasterResponse();
        //    try
        //    {

        //        if (CMSId != null && CMSId.HasValue)
        //        {
        //            if (CMSId == 0)
        //                return response;
        //            using (ColspEntities db = new ColspEntities())
        //            {
        //                var GetCMS = db.CMSMasters.Where(c => c.CMSMasterId == CMSId).FirstOrDefault();
        //                if (GetCMS != null)
        //                {
        //                    response.CMSMasterId = GetCMS.CMSMasterId;
        //                    string CreateBy = string.Empty;
        //                    if (GetCMS.CreateBy.HasValue)
        //                    {
        //                        var by = db.Users.Where(c => c.UserId == GetCMS.CreateBy).FirstOrDefault();
        //                        if (by != null)
        //                            CreateBy = by.NameEn;
        //                    }
        //                    response.CreateBy = CreateBy;
        //                    response.CMSMasterNameEN = GetCMS.CMSMasterNameEN;
        //                    response.CMSMasterNameTH = GetCMS.CMSMasterNameTH;
        //                    string CMSType = string.Empty;
        //                    if (GetCMS.CreateBy.HasValue)
        //                    {
        //                        var Type = db.CMSMasterTypes.Where(c => c.CMSMasterTypeId == GetCMS.CMSTypeId).FirstOrDefault();
        //                        if (Type != null)
        //                            CMSType = Type.CMSMasterTypeNameEN;
        //                    }
        //                    response.CMSType = CMSType;
        //                    response.CMSMasterEffectiveDate = GetCMS.CMSMasterEffectiveDate;
        //                    response.CMSMasterEffectiveTime = GetCMS.CMSMasterEffectiveTime;
        //                    response.CMSMasterExpiryDate = GetCMS.CMSMasterExpiryDate;
        //                    response.CMSMasterExpiryTime = GetCMS.CMSMasterExpiryTime;
        //                    response.CreateIP = GetCMS.CreateIP;
        //                    response.LongDescriptionEN = GetCMS.LongDescriptionEN;
        //                    response.LongDescriptionTH = GetCMS.LongDescriptionTH;
        //                    response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
        //                    response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
        //                    response.CMSMasterStatusId = GetCMS.CMSMasterStatusId;
        //                    string Status = string.Empty;
        //                    if (GetCMS.CMSMasterStatusId.HasValue)
        //                    {
        //                        var st = db.CMSMasterStatus.Where(c => c.CMSMasterStatusId == GetCMS.CMSMasterStatusId).FirstOrDefault();
        //                        if (st != null)
        //                            Status = st.CMSMasterStatusNameEN;
        //                    }
        //                    response.CMSStatus = Status;
        //                    response.CMSMasterURLKey = GetCMS.CMSMasterURLKey;
        //                    response.Visibility = GetCMS.Visibility;
        //                    response.CreateDate = (DateTime)GetCMS.Createdate;

        //                    List<CategoryListResponse> CategoryList = new List<CategoryListResponse>();
        //                    var CategoryLists = (from map in db.CMSMastserCategoryMaps.Where(m => m.CMSMasterId == CMSId)
        //                                         from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
        //                                         select new
        //                                         {
        //                                             CMSMasterId = map.CMSMasterId,
        //                                             CMSCategoryId = map.CMSCategoryId,
        //                                             CategoryNameEN = cat.CMSCategoryNameEN,
        //                                             CategoryNameTH = cat.CMSCategoryNameTH
        //                                         });
        //                    int CountItem = 0;
        //                    foreach (var itemCat in CategoryLists)
        //                    {
        //                        CategoryListResponse model = new CategoryListResponse();
        //                        model.CMSMasterId = GetCMS.CMSMasterId;
        //                        model.CMSCategoryId = itemCat.CMSCategoryId;
        //                        List<ProductListResponse> ProductList = new List<ProductListResponse>();
        //                        var ProductLists = (from map in db.CMSCategoryProductMaps.Where(m => m.CMSCategoryId == itemCat.CMSCategoryId)
        //                                            from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
        //                                            from pro in db.Products.Where(c => c.Pid == map.ProductPID).DefaultIfEmpty()
        //                                            select new ProductListResponse
        //                                            {
        //                                                CMSCategoryId = (int)map.CMSCategoryId,
        //                                                ProductNameEN = pro.ProductNameEn,
        //                                                ProductNameTH = pro.ProductNameTh
        //                                            });
        //                        CategoryList.Add(model);
        //                        CountItem++;
        //                    }
        //                    response.CategoryLists = CategoryList;
        //                    return response;
        //                }
        //                else
        //                {
        //                    return response;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return response;
        //    }
        //}
        #endregion

        #region Search

        #endregion


        #region Edit/Update CMS
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
                return Request.CreateErrorResponse(HttpStatusCode.OK, "Update Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }


        }


        //[Route("api/CMSGroupUpdate")]
        //[HttpPost]
        //public HttpResponseMessage CMSGroupUpdate(CMSGroupRequest model)
        //{
        //    try
        //    {
        //        if (model != null)
        //        {
        //            int CMSGroupId = 0;
        //            CMSProcess cmsGroup = new CMSProcess();
        //            CMSGroupId = cmsGroup.EditCMSGroup(model);
        //            return GetCMSGroup(CMSGroupId);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }


        //}
        #endregion

        #region export

        //[Route("api/CMSStages/Export")]
        //[HttpPost]
        //public HttpResponseMessage ExportCollection(List<CMSCollectionItemRequest> request)
        //{
        //    MemoryStream stream = null;
        //    StreamWriter writer = null;
        //    try
        //    {
        //        stream = new MemoryStream();
        //        writer = new StreamWriter(stream);
        //        string header = @"Collection ID,Collection English Name,Collection Thai Name,URL Keyword,EffectiveDate,EffectiveTime,ExpiryDate,ExpiryTime,ShopId,CMSCount,ShortDescriptionTH,ShortDescriptionEN,LongDescriptionTH,LongDescriptionEN,Sequence,Collection Status,Visibility";
        //        writer.WriteLine(header);
        //        StringBuilder sb = null;
        //        foreach (CMSCollectionItemRequest rq in request)
        //        {
        //            sb = new StringBuilder();
        //            if (rq.CMSId == default(int)) { throw new Exception("Collection Id cannot be null"); }
        //            var coll = db.CMSMasters.Find(rq.CMSId);
        //            var cmsFlowStatus = db.CMSStatusFlows.Find(coll.CMSStatusFlowId);
        //            var visible = coll.Visibility != null ? (coll.Visibility == true ? "Visible" : "InVisible") : "unknow";
        //            if (coll == null)
        //            {
        //                throw new Exception("Cannot find Collection with id " + rq.CMSId);
        //            }
        //            sb.Append(coll.CMSId); sb.Append(",");
        //            sb.Append(coll.CMSNameEN); sb.Append(",");
        //            sb.Append(coll.CMSNameTH); sb.Append(",");
        //            sb.Append(coll.URLKey); sb.Append(",");
        //            //sb.Append(coll.CMSTypeId); sb.Append(",");
        //            //sb.Append(coll.CMSFilterId); sb.Append(",");
        //            sb.Append(coll.EffectiveDate); sb.Append(",");
        //            sb.Append(coll.EffectiveTime); sb.Append(",");
        //            sb.Append(coll.ExpiryDate); sb.Append(",");
        //            sb.Append(coll.ExpiryTime); sb.Append(",");
        //            sb.Append(coll.ShopId); sb.Append(",");
        //            sb.Append(coll.CMSCount); sb.Append(",");
        //            sb.Append(coll.ShortDescriptionTH); sb.Append(",");
        //            sb.Append(coll.ShortDescriptionEN); sb.Append(",");
        //            if (!string.IsNullOrEmpty(coll.LongDescriptionTH))
        //            {
        //                if (coll.LongDescriptionTH.Contains("\""))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH.Replace("\"", "\"\""));
        //                }
        //                if (coll.LongDescriptionTH.Contains(","))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
        //                }
        //                if (coll.LongDescriptionTH.Contains(System.Environment.NewLine))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(coll.LongDescriptionEN))
        //            {
        //                if (coll.LongDescriptionEN.Contains("\""))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN.Replace("\"", "\"\""));
        //                }
        //                if (coll.LongDescriptionEN.Contains(","))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
        //                }
        //                if (coll.LongDescriptionEN.Contains(System.Environment.NewLine))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
        //                }
        //            }
        //            sb.Append("\"" + coll.LongDescriptionTH + "\""); sb.Append(",");
        //            sb.Append("\"" + coll.LongDescriptionEN + "\""); sb.Append(",");
        //            sb.Append(coll.Sequence); sb.Append(",");
        //            //sb.Append(coll.CMSCollectionGroupId); sb.Append(",");
        //            sb.Append(cmsFlowStatus.CMSStatusName); sb.Append(",");
        //            sb.Append(visible); sb.Append(",");
        //            //sb.Append(coll.CreateBy); sb.Append(",");
        //            //sb.Append(coll.Createdate); sb.Append(",");
        //            //sb.Append(coll.UpdateBy); sb.Append(",");
        //            //sb.Append(coll.UpdateDate); sb.Append(",");
        //            //sb.Append(coll.CreateIP); sb.Append(",");
        //            //sb.Append(coll.UpdateIP); sb.Append(",");


        //            writer.WriteLine(sb);
        //        }
        //        writer.Flush();
        //        stream.Position = 0;

        //        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //        result.Content = new StreamContent(stream);
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
        //        {
        //            CharSet = Encoding.UTF8.WebName
        //        };
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        result.Content.Headers.ContentDisposition.FileName = "file.csv";
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        if (writer != null)
        //        {
        //            writer.Close();
        //            writer.Dispose();
        //        }
        //        if (stream != null)
        //        {
        //            stream.Close();
        //            stream.Dispose();
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e);
        //    }
        //}



        //[Route("api/CMSStages/ExportGroup")]
        //[HttpPost]
        //public HttpResponseMessage ExportCollectionGroup(List<CMSCollectionItemRequest> request)
        //{
        //    MemoryStream stream = null;
        //    StreamWriter writer = null;
        //    try
        //    {
        //        stream = new MemoryStream();
        //        writer = new StreamWriter(stream);
        //        string header = @"Collection ID,Collection English Name,Collection Thai Name,URL Keyword,EffectiveDate,EffectiveTime,ExpiryDate,ExpiryTime,ShopId,CMSCount,ShortDescriptionTH,ShortDescriptionEN,LongDescriptionTH,LongDescriptionEN,Sequence,Collection Status,Visibility";
        //        writer.WriteLine(header);
        //        StringBuilder sb = null;
        //        foreach (CMSCollectionItemRequest rq in request)
        //        {
        //            sb = new StringBuilder();
        //            if (rq.CMSId == default(int)) { throw new Exception("Collection Id cannot be null"); }
        //            var coll = db.CMSMasters.Find(rq.CMSId);
        //            var cmsFlowStatus = db.CMSStatusFlows.Find(coll.CMSStatusFlowId);
        //            var visible = coll.Visibility != null ? (coll.Visibility == true ? "Visible" : "InVisible") : "unknow";
        //            if (coll == null)
        //            {
        //                throw new Exception("Cannot find Collection with id " + rq.CMSId);
        //            }
        //            sb.Append(coll.CMSId); sb.Append(",");
        //            sb.Append(coll.CMSNameEN); sb.Append(",");
        //            sb.Append(coll.CMSNameTH); sb.Append(",");
        //            sb.Append(coll.URLKey); sb.Append(",");
        //            //sb.Append(coll.CMSTypeId); sb.Append(",");
        //            //sb.Append(coll.CMSFilterId); sb.Append(",");
        //            sb.Append(coll.EffectiveDate); sb.Append(",");
        //            sb.Append(coll.EffectiveTime); sb.Append(",");
        //            sb.Append(coll.ExpiryDate); sb.Append(",");
        //            sb.Append(coll.ExpiryTime); sb.Append(",");
        //            sb.Append(coll.ShopId); sb.Append(",");
        //            sb.Append(coll.CMSCount); sb.Append(",");
        //            sb.Append(coll.ShortDescriptionTH); sb.Append(",");
        //            sb.Append(coll.ShortDescriptionEN); sb.Append(",");
        //            if (!string.IsNullOrEmpty(coll.LongDescriptionTH))
        //            {
        //                if (coll.LongDescriptionTH.Contains("\""))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH.Replace("\"", "\"\""));
        //                }
        //                if (coll.LongDescriptionTH.Contains(","))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
        //                }
        //                if (coll.LongDescriptionTH.Contains(System.Environment.NewLine))
        //                {
        //                    coll.LongDescriptionTH = String.Format("\"{0}\"", coll.LongDescriptionTH);
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(coll.LongDescriptionEN))
        //            {
        //                if (coll.LongDescriptionEN.Contains("\""))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN.Replace("\"", "\"\""));
        //                }
        //                if (coll.LongDescriptionEN.Contains(","))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
        //                }
        //                if (coll.LongDescriptionEN.Contains(System.Environment.NewLine))
        //                {
        //                    coll.LongDescriptionEN = String.Format("\"{0}\"", coll.LongDescriptionEN);
        //                }
        //            }
        //            sb.Append("\"" + coll.LongDescriptionTH + "\""); sb.Append(",");
        //            sb.Append("\"" + coll.LongDescriptionEN + "\""); sb.Append(",");
        //            sb.Append(coll.Sequence); sb.Append(",");
        //            //sb.Append(coll.CMSCollectionGroupId); sb.Append(",");
        //            sb.Append(cmsFlowStatus.CMSStatusName); sb.Append(",");
        //            sb.Append(visible); sb.Append(",");
        //            //sb.Append(coll.CreateBy); sb.Append(",");
        //            //sb.Append(coll.Createdate); sb.Append(",");
        //            //sb.Append(coll.UpdateBy); sb.Append(",");
        //            //sb.Append(coll.UpdateDate); sb.Append(",");
        //            //sb.Append(coll.CreateIP); sb.Append(",");
        //            //sb.Append(coll.UpdateIP); sb.Append(",");


        //            writer.WriteLine(sb);
        //        }
        //        writer.Flush();
        //        stream.Position = 0;

        //        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //        result.Content = new StreamContent(stream);
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
        //        {
        //            CharSet = Encoding.UTF8.WebName
        //        };
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        result.Content.Headers.ContentDisposition.FileName = "file.csv";
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        if (writer != null)
        //        {
        //            writer.Close();
        //            writer.Dispose();
        //        }
        //        if (stream != null)
        //        {
        //            stream.Close();
        //            stream.Dispose();
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e);
        //    }
        //}

        #endregion


        #region new CMS CATE/COL/GROUP

        //[Route("api/CMSCategoryList")]
        //[HttpGet]
        //public IHttpActionResult GetCategoryList([FromUri] GeneralSearchRequest request)
        //{
        //    //int? shopId = 0;
        //    try
        //    {
        //        IQueryable<CMSCollectionCategory> CMS;
        //        CMS = (from c in db.CMSCollectionCategories
        //               where c.Status == true
        //               select c
        //                  );
        //        if (request == null)
        //        {
        //            return Ok(CMS);
        //        }
        //        request.DefaultOnNull();
        //        if (!string.IsNullOrEmpty(request.SearchText))
        //        {
        //            CMS = CMS.Where(c => (c.CMSCollectionCategoryNameEN.Contains(request.SearchText) || c.CMSCollectionCategoryNameTH.Contains(request.SearchText)));
        //        }
        //        if (!string.IsNullOrEmpty(request._filter))
        //        {

        //            if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_DRAFT);
        //            }
        //            else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_APPROVE);
        //            }
        //            else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_NOT_APPROVE);
        //            }
        //            else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
        //            }
        //        }
        //        var total = CMS.Count();
        //        var pagedCMS = CMS.Paginate(request);
        //        var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(request);
        //    }
        //}

        //[Route("api/CMSCollectionList")]
        //[HttpGet]
        //public IHttpActionResult GetCollectionList([FromUri] GeneralSearchRequest request)
        //{
        //    //int? shopId = 0;
        //    try
        //    {
        //        IQueryable<CMSMaster> CMS;
        //        CMS = (from c in db.CMSMasters
        //               where c.Status == true
        //               select c
        //                  );
        //        if (request == null)
        //        {
        //            return Ok(CMS);
        //        }
        //        request.DefaultOnNull();
        //        if (!string.IsNullOrEmpty(request.SearchText))
        //        {
        //            CMS = CMS.Where(c => (c.CMSNameEN.Contains(request.SearchText) || c.CMSNameTH.Contains(request.SearchText)));
        //        }
        //        if (!string.IsNullOrEmpty(request._filter))
        //        {

        //            if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_DRAFT);
        //            }
        //            else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_APPROVE);
        //            }
        //            else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_NOT_APPROVE);
        //            }
        //            else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
        //            }
        //        }
        //        var total = CMS.Count();
        //        var pagedCMS = CMS.Paginate(request);
        //        var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(request);
        //    }
        //}

        //[Route("api/CMSGroupList")]
        //[HttpGet]
        //public IHttpActionResult GetGroupList([FromUri] GeneralSearchRequest request)
        //{
        //    //int? shopId = 0;
        //    try
        //    {                
        //        IQueryable<CMSCollectionGroup> CMS;
        //        CMS = (from c in db.CMSCollectionGroups
        //               where c.Status == true
        //               select c                       
        //                  );
        //        if (request == null)
        //        {
        //            return Ok(CMS);
        //        }
        //        request.DefaultOnNull();
        //        if (!string.IsNullOrEmpty(request.SearchText))
        //        {
        //            CMS = CMS.Where(c => (c.CMSCollectionGroupNameEN.Contains(request.SearchText) || c.CMSCollectionGroupNameTH.Contains(request.SearchText)));
        //        }
        //        if (!string.IsNullOrEmpty(request._filter))
        //        {

        //            if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_DRAFT);
        //            }
        //            else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_APPROVE);
        //            }
        //            else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_NOT_APPROVE);
        //            }
        //            else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
        //            {
        //                CMS = CMS.Where(p => p.CMSStatusFlowId == Constant.CMS_STATUS_WAIT_FOR_APPROVAL);
        //            }
        //        }
        //        var total = CMS.Count();
        //        var pagedCMS = CMS.Paginate(request);
        //        var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(request);
        //    }

        //}


        //[Route("api/CMSCategoryAdd")]
        //[HttpPost]
        //public HttpResponseMessage CMSCategoryAdd(CMSCollectionCategoryRequest model)
        //{
        //    try
        //    {
        //        if (model != null)
        //        {
        //            int CategoryId = 0;
        //            CMSProcess cms = new CMSProcess();
        //            CategoryId = cms.CreateCMSCollectionCategory(model);
        //            return GetCollectionCategory(CategoryId);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}


        //[Route("api/CMSCategory/{CategoryId}")]
        //[HttpGet]
        //public HttpResponseMessage GetCollectionCategory(int? CategoryId)
        //{
        //    try
        //    {
        //        CMSCollectionCategoryResponse response = new CMSCollectionCategoryResponse();
        //        if (CategoryId != null && CategoryId.HasValue)
        //        {
        //            if (CategoryId == 0)
        //                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CategoryId ID is invalid. Cannot find CategoryId in System");
        //            using (ColspEntities db = new ColspEntities())
        //            {
        //                var GetCMS = db.CMSCollectionCategories.Where(c => c.CMSCollectionCategoryId == CategoryId && c.Status == true).FirstOrDefault();
        //                if (GetCMS != null)
        //                {
        //                    response.CMSCollectionCategoryId        = GetCMS.CMSCollectionCategoryId        ;
        //                    response.CMSCollectionCategoryNameEH    = GetCMS.CMSCollectionCategoryNameEN    ;
        //                    response.CMSCollectionCategoryNameTH    = GetCMS.CMSCollectionCategoryNameTH    ;
        //                    response.Status                         = GetCMS.Status                         ;
        //                    response.Visibility                     = GetCMS.Visibility                     ;
        //                    response.CreateBy                       = GetCMS.CreateBy                       ;
        //                    response.Createdate                     = GetCMS.Createdate                     ;
        //                    response.UpdateBy                       = GetCMS.UpdateBy                       ;
        //                    response.UpdateDate                     = GetCMS.UpdateDate                     ;
        //                    response.CreateIP                       = GetCMS.CreateIP                       ;
        //                    response.UpdateIP                       = GetCMS.UpdateIP                       ;
        //                    response.EffectiveDate                  = GetCMS.EffectiveDate                  ;
        //                    response.EffectiveTime                  = GetCMS.EffectiveTime                  ;
        //                    response.ExpiryDate                     = GetCMS.ExpiryDate                     ;
        //                    response.ExpiryTime                     = GetCMS.ExpiryTime                     ;
        //                    response.CMSStatusFlowId                = GetCMS.CMSStatusFlowId                ;
        //                    response.Sequence                       = GetCMS.Sequence                       ;

        //                    List<CMSRelProductCategoryResponse> CMSRelProductCategory = new List<CMSRelProductCategoryResponse>();
        //                    var tmpItemList = db.CMSRelProductCategories.Where(c => c.CMSCollectionCategoryId == CategoryId).ToList();

        //                    foreach (var itemCollection in tmpItemList)
        //                    {
        //                        CMSRelProductCategoryResponse model = new CMSRelProductCategoryResponse();
        //                      model.CMSRelProductCategoryId     = itemCollection.CMSRelProductCategoryId        ;
        //                      model.ProductId                   = itemCollection.ProductId                      ;
        //                      model.Status                      = itemCollection.Status                         ;
        //                      model.Sequence                    = itemCollection.Sequence                       ;
        //                      model.Visibility                  = itemCollection.Visibility                     ;
        //                      model.CreateBy                    = itemCollection.CreateBy                       ;
        //                      model.Createdate                  = itemCollection.Createdate                     ;
        //                      model.UpdateBy                    = itemCollection.UpdateBy                       ;
        //                      model.UpdateDate                  = itemCollection.UpdateDate                     ;
        //                      model.CreateIP                    = itemCollection.CreateIP                       ;
        //                      model.UpdateIP                    = itemCollection.UpdateIP                       ;
        //                      model.EffectiveDate               = itemCollection.EffectiveDate                  ;
        //                      model.EffectiveTime               = itemCollection.EffectiveTime                  ;
        //                      model.ExpiryDate                  = itemCollection.ExpiryDate                     ;
        //                      model.ExpiryTime                  = itemCollection.ExpiryTime                     ;
        //                      model.CMSCollectionCategoryId = itemCollection.CMSCollectionCategoryId          ;

        //                        CMSRelProductCategory.Add(model);
        //                    }
        //                    response.CMSRelProductCategory = CMSRelProductCategory;
        //                    return Request.CreateResponse(HttpStatusCode.OK, response);
        //                }
        //                else
        //                {
        //                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find CMS ID in System");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}


        //[Route("api/CMSCategoryEditItem")]
        //[HttpPost]
        //public HttpResponseMessage CMSCategoryEditItem(CMSCollectionCategoryRequest model)
        //{
        //    try
        //    {
        //        int CMSId = 0;
        //        if (model != null)
        //        {                   
        //                    CMSProcess cms = new CMSProcess();
        //                    CMSId = cms.EditCMSCollectionCategory(model);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.OK, "Edit Complete");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}


        //[Route("api/CMSCategoryEditList")]
        //[HttpPost]
        //public HttpResponseMessage CMSCategoryEditList(List<CMSCollectionCategoryRequest> model)
        //{
        //    try
        //    {
        //        int CMSId = 0;
        //        if (model != null)
        //        {
        //            foreach (var item in model)
        //            {
        //                CMSProcess cms = new CMSProcess();
        //                CMSId = cms.EditCMSCollectionCategory(item);
        //            };
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.OK, "Edit Complete");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}


        //[Route("api/CMSCategoryUpdateItem")]
        //[HttpPost]
        //public HttpResponseMessage CMSCategoryUpdateItem(CMSCollectionCategoryRequest model)
        //{
        //    try
        //    {
        //        int CMSId = 0;
        //        if (model != null)
        //        {
        //            CMSProcess cms = new CMSProcess();
        //            CMSId = cms.UpdateCMSCollectionCategory(model);
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.OK, "Update Complete");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}


        //[Route("api/CMSCategoryUpdateList")]
        //[HttpPost]
        //public HttpResponseMessage CMSCategoryUpdateList(List<CMSCollectionCategoryRequest> model)
        //{
        //    try
        //    {
        //        int CMSId = 0;
        //        if (model != null)
        //        {
        //            foreach (var item in model) {
        //                CMSProcess cms = new CMSProcess();
        //                CMSId = cms.UpdateCMSCollectionCategory(item);
        //            };
        //        }
        //        else
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
        //        }
        //        return Request.CreateErrorResponse(HttpStatusCode.OK, "Update Complete");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}
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
