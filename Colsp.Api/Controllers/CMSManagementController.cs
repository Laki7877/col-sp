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
    public class CMSListResponse
    {
        public int CMSMasterId { get; set; }
        public string CMSMasterNameEN { get; set; }
        public string CMSMasterNameTH { get; set; }
        public string CMSMasterGroupName { get; set; }
        public Nullable<DateTime> CMSMasterEffectiveDate { get; set; }
        public Nullable<DateTime> CMSMasterExpiryDate { get; set; }
        public int? CMSMasterStatusId { get; set; }
        public CMSListResponse()
        {
            CMSMasterId = 0;
            CMSMasterNameEN = string.Empty;
            CMSMasterNameTH = string.Empty;
            CMSMasterGroupName = string.Empty;
            CMSMasterEffectiveDate = null;
            CMSMasterExpiryDate = null;
            CMSMasterStatusId = 0;
        }
    }
    public class CMSManagementController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        #region Get List 
        [Route("api/CMS/Master")]
        [HttpGet]
        public IHttpActionResult GetCMSALL([FromUri] CMSMasterAllRequest request)
        {
            try
            {
                IQueryable<CMSListResponse> CMS;
                dynamic response = string.Empty;
                if (this.User.UserRequest().Type == Constant.USER_TYPE_ADMIN)
                {
                    CMS = (from m in db.CMSMasters
                           from gm in db.CMSMasterGroupMaps.Where(y => y.CMSMasterId == m.CMSMasterId).DefaultIfEmpty()
                           from g in db.CMSGroups.Where(z => z.CMSGroupId == gm.CMSMasterGroupId).DefaultIfEmpty()
                           from t in db.CMSMasterTypes.Where(x => x.CMSMasterTypeId == m.CMSTypeId).DefaultIfEmpty()
                           select new CMSListResponse
                           {
                               CMSMasterId = m.CMSMasterId,
                               CMSMasterNameEN = m.CMSMasterNameEN,
                               CMSMasterNameTH = m.CMSMasterNameTH,
                               CMSMasterGroupName = g.CMSGroupNameEN,
                               CMSMasterEffectiveDate = m.CMSMasterEffectiveDate,
                               CMSMasterExpiryDate = m.CMSMasterExpiryDate,
                           }).Take(100);
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

                    CMS = (from m in db.CMSMasters
                           from gm in db.CMSMasterGroupMaps.Where(y => y.CMSMasterId == m.CMSMasterId).DefaultIfEmpty()
                           from g in db.CMSGroups.Where(z => z.CMSGroupId == gm.CMSMasterGroupId).DefaultIfEmpty()
                           from t in db.CMSMasterTypes.Where(x => x.CMSMasterTypeId == m.CMSTypeId).DefaultIfEmpty()
                           select new CMSListResponse
                           {
                               CMSMasterId = m.CMSMasterId,
                               CMSMasterNameEN = m.CMSMasterNameEN,
                               CMSMasterNameTH = m.CMSMasterNameTH,
                               CMSMasterGroupName = g.CMSGroupNameEN,
                               CMSMasterEffectiveDate = m.CMSMasterEffectiveDate,
                               CMSMasterExpiryDate = m.CMSMasterExpiryDate,
                           }).Take(100);

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

        [Route("api/CMS/GetAllGroup")]
        [HttpGet]
        public IHttpActionResult GetAllGroup([FromUri] CMSGroupListRequest request)
        {
            try
            {
                IQueryable<CMSGroup> CMS;
                dynamic response = string.Empty;
                if (this.User.UserRequest().Type == Constant.USER_TYPE_ADMIN)
                {
                    CMS = (from c in db.CMSGroups
                           select c
                               ).Take(100);
                    if (request == null)
                    {
                        return Ok(CMS);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrEmpty(request.SearchText))
                    {
                        CMS = CMS.Where(c => (c.CMSGroupNameEN.Contains(request.SearchText) || c.CMSGroupNameTH.Contains(request.SearchText)));
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

                    CMS = (from c in db.CMSMasterGroupMaps.Where(x => x.ShopId == shopId)
                           from m in db.CMSGroups.Where(m => m.CMSGroupId == c.CMSMasterGroupId).DefaultIfEmpty()
                           select m
                          ).Take(100);

                    if (request == null)
                    {
                        return Ok(CMS);
                    }
                    request.DefaultOnNull();
                    if (!string.IsNullOrEmpty(request.SearchText))
                    {
                        CMS = CMS.Where(c => (c.CMSGroupNameEN.Contains(request.SearchText) || c.CMSGroupNameTH.Contains(request.SearchText)));
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
        #endregion

        #region Create
        [Route("api/CMS/Master/Create")]
        [HttpPost]
        public HttpResponseMessage CreateCMSMaster(CMSMasterRequest model)
        {
            try
            {
                if (model != null)
                {
                    int CMSId = 0;

                    CMSProcess cms = new CMSProcess();
                    CMSId = cms.CreateCMSMaster(model);

                    return GetCMSMasterDetail(CMSId);
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

        [Route("api/CMS/Group/Create")]
        [HttpPost]
        public HttpResponseMessage CMSGroupCreate(CMSGroupRequest model)
        {
            try
            {
                if (model != null)
                {
                    int CMSGroupId = 0;
                    CMSProcess cmsGroup = new CMSProcess();
                    CMSGroupId = cmsGroup.CreateCMSGroup(model);
                    return GetCMSGroup(CMSGroupId);
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
        #endregion

        #region Mapping
        [Route("api/CMS/Master/MasterCMSCategoryMap")]
        [HttpPost]
        public HttpResponseMessage MasterCMSCategoryMap(CMSMasterMappingItemCategoryRequest model)
        {
            try
            {
                int CMSId = 0;
                if (model != null)
                {
                    var getMaster = db.CMSMasters.Where(c => c.CMSMasterId == model.CMSMasterId).FirstOrDefault();
                    if (getMaster != null)
                    {
                        CMSId = getMaster.CMSMasterId;
                        //Create Scheduler
                        CMSProcess Process = new CMSProcess();
                        int SchedulerId = Process.CreateCMSScheduler(getMaster, User.UserRequest().UserId, User.ShopRequest().ShopId);
                        //map category
                        if (SchedulerId != 0)
                        {
                            foreach (var itemCat in model.CategoryList)
                            {
                                CMSCategorySchedulerMap CatMapScheduler = new CMSCategorySchedulerMap();
                                CatMapScheduler.CMSCategoryId = itemCat.CMSCategoryId;
                                CatMapScheduler.CMSSchedulerId = SchedulerId;
                                CatMapScheduler.CreateBy = User.UserRequest().UserId;
                                CatMapScheduler.Createdate = DateTime.Now;
                                CatMapScheduler.CreateIP = model.CreateIP;
                                db.CMSCategorySchedulerMaps.Add(CatMapScheduler);
                                db.SaveChanges();
                            }
                        }
                    }
                    return GetCMSMasterDetail(CMSId);
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
        #endregion

        #region Get View Detail
        [Route("api/CMS/Group/{CMSGroupId}")]
        [HttpPost]
        public HttpResponseMessage GetCMSGroup(int? CMSGroupId)
        {
            try
            {
                CMSGroupResponse response = new CMSGroupResponse();
                if (CMSGroupId != null && CMSGroupId.HasValue)
                {
                    if (CMSGroupId == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CMS ID is invalid. Cannot find CMSId in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var GetCMS = db.CMSGroups.Where(c => c.CMSGroupId == CMSGroupId).FirstOrDefault();
                        if (GetCMS != null)
                        {
                            response.CMSGroupId = GetCMS.CMSGroupId;
                            response.CreateBy = GetCMS.CreateBy;
                            response.CMSGroupNameEN = GetCMS.CMSGroupNameEN;
                            response.CMSGroupNameTH = GetCMS.CMSGroupNameTH;
                            response.Sequence = (int)GetCMS.Sequence;
                            List<CMSMasterResponse> MasterList = new List<CMSMasterResponse>();
                            var GetCMSMasterIdList = db.CMSMasterGroupMaps.Where(c => c.CMSMasterGroupId == GetCMS.CMSGroupId).Select(c => c.CMSMasterId).ToList();
                            foreach (int CMSMasteritem in GetCMSMasterIdList)
                            {
                                CMSMasterResponse CMSMaster = new CMSMasterResponse();
                                CMSMaster = this.GetCMSMasterDetailById(CMSMasteritem);
                                MasterList.Add(CMSMaster);
                            }
                            response.CMSMasterList = MasterList;

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

        [Route("api/CMS/Master/{CMSMasterId}")]
        [HttpGet]
        public HttpResponseMessage GetCMSMasterDetail(int? CMSId)
        {
            try
            {
                CMSMasterResponse response = new CMSMasterResponse();
                if (CMSId != null && CMSId.HasValue)
                {
                    if (CMSId == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "CMS ID is invalid. Cannot find CMSId in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var GetCMS = db.CMSMasters.Where(c => c.CMSMasterId == CMSId).FirstOrDefault();
                        if (GetCMS != null)
                        {
                            response.CMSMasterId = GetCMS.CMSMasterId;
                            string CreateBy = string.Empty;
                            if (GetCMS.CreateBy.HasValue)
                            {
                                var by = db.Users.Where(c => c.UserId == GetCMS.CreateBy).FirstOrDefault();
                                if (by != null)
                                    CreateBy = by.NameEn;
                            }
                            response.CreateBy = CreateBy;
                            response.CMSMasterNameEN = GetCMS.CMSMasterNameEN;
                            response.CMSMasterNameTH = GetCMS.CMSMasterNameTH;
                            string CMSType = string.Empty;
                            if (GetCMS.CreateBy.HasValue)
                            {
                                var Type = db.CMSMasterTypes.Where(c => c.CMSMasterTypeId == GetCMS.CMSTypeId).FirstOrDefault();
                                if (Type != null)
                                    CMSType = Type.CMSMasterTypeNameEN;
                            }
                            response.CMSType = CMSType;
                            response.CMSMasterEffectiveDate = GetCMS.CMSMasterEffectiveDate;
                            response.CMSMasterEffectiveTime = GetCMS.CMSMasterEffectiveTime;
                            response.CMSMasterExpiryDate = GetCMS.CMSMasterExpiryDate;
                            response.CMSMasterExpiryTime = GetCMS.CMSMasterExpiryTime;
                            response.CreateIP = GetCMS.CreateIP;
                            response.LongDescriptionEN = GetCMS.LongDescriptionEN;
                            response.LongDescriptionTH = GetCMS.LongDescriptionTH;
                            response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
                            response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
                            response.CMSMasterStatusId = GetCMS.CMSMasterStatusId;
                            string Status = string.Empty;
                            if (GetCMS.CMSMasterStatusId.HasValue)
                            {
                                var st = db.CMSMasterStatus.Where(c => c.CMSMasterStatusId == GetCMS.CMSMasterStatusId).FirstOrDefault();
                                if (st != null)
                                    Status = st.CMSMasterStatusNameEN;
                            }
                            response.CMSStatus = Status;
                            response.CMSMasterURLKey = GetCMS.CMSMasterURLKey;
                            response.Visibility = GetCMS.Visibility;
                            response.CreateDate = (DateTime)GetCMS.Createdate;

                            #region Collection


                            List<SchedulerListResponse> SchedulerList = new List<SchedulerListResponse>();
                            var SchedulerLists = (from map in db.CMSMasterSchedulerMaps.Where(c => c.CMSMasterId == CMSId)
                                                  from sch in db.CMSSchedulers.Where(c => c.CMSSchedulerId == map.CMSSchedulerId).DefaultIfEmpty()
                                                  select new
                                                  {
                                                      map.CMSMasterId,
                                                      map.CMSSchedulerId,
                                                      map.CMSMasterSchedulerMapId,
                                                      sch.EffectiveDate,
                                                      sch.EffectiveTime,
                                                      sch.ExpiryDate,
                                                      sch.ExpiryTime
                                                  });
                            #region Scheduler
                            foreach (var Scheduleritem in SchedulerLists)
                            {
                                SchedulerListResponse SchedulerModel = new SchedulerListResponse();
                                List<CategoryListResponse> CategoryList = new List<CategoryListResponse>();
                                var CategoryLists = (from map in db.CMSCategorySchedulerMaps.Where(m => m.CMSSchedulerId == Scheduleritem.CMSSchedulerId)
                                                     from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
                                                     select new
                                                     {
                                                         CMSCategoryId = map.CMSCategoryId,
                                                         CategoryNameEN = cat.CMSCategoryNameEN,
                                                         CategoryNameTH = cat.CMSCategoryNameTH
                                                     });
                                SchedulerModel.CMSSchedulerId = (int)Scheduleritem.CMSSchedulerId;
                                SchedulerModel.EffectiveDate = Scheduleritem.EffectiveDate;
                                SchedulerModel.EffectiveTime = Scheduleritem.EffectiveTime;
                                SchedulerModel.ExpiryDate = Scheduleritem.ExpiryDate;
                                SchedulerModel.ExpiryTime = Scheduleritem.ExpiryTime;
                                #region Cateegory
                                foreach (var itemCat in CategoryLists)
                                {
                                    CategoryListResponse CategoryModel = new CategoryListResponse();
                                    CategoryModel.CMSMasterId = GetCMS.CMSMasterId;
                                    CategoryModel.CMSCategoryId = (int)itemCat.CMSCategoryId;
                                    CategoryModel.CategoryNameEN = itemCat.CategoryNameEN;
                                    CategoryModel.CategoryNameTH = itemCat.CategoryNameTH;
                                    List<CriteriaListResponse> CriteriaList = new List<CriteriaListResponse>();
                                    var CriteriaLists = (from crc in db.CMSCategoryCreteriaMaps.Where(c => c.CMSCategoryId == itemCat.CMSCategoryId)
                                                         from cr in db.CMSCreterias.Where(c => c.CMSCreteriaId == crc.CMSCreteriaId).DefaultIfEmpty()
                                                         select new
                                                         {
                                                             cr.CMSCreteriaId,
                                                             cr.CategoryId,
                                                             cr.Brand,
                                                             cr.MinPrice,
                                                             cr.MaxPrice

                                                         });
                                    #region Criteria
                                    foreach (var itemCriteria in CriteriaLists)
                                    {
                                        CriteriaListResponse CriteriaModel = new CriteriaListResponse();
                                        List<ProductListResponse> ProductList = new List<ProductListResponse>();
                                        var ProductLists = (from map in db.CMSCriteriaProductMaps.Where(m => m.CMSCriteriaId == itemCriteria.CMSCreteriaId)
                                                            from pro in db.Products.Where(c => c.Pid == map.ProductPID).DefaultIfEmpty()
                                                            select new
                                                            {
                                                                map.ProductPID,
                                                                pro.ProductNameEn,
                                                                pro.ProductNameTh,
                                                                pro.SalePrice
                                                            });
                                        CriteriaModel.Brand = itemCriteria.Brand;
                                        CriteriaModel.CategoryId = itemCriteria.CategoryId;
                                        CriteriaModel.MaxPrice = itemCriteria.MaxPrice;
                                        CriteriaModel.MinPrice = itemCriteria.MinPrice;
                                        //list
                                        #region Product
                                        foreach (var itemProduct in ProductLists)
                                        {
                                            ProductListResponse pro = new ProductListResponse();
                                            pro.ProductPId = itemProduct.ProductPID;
                                            pro.ProductNameEN = itemProduct.ProductNameEn;
                                            pro.ProductNameTH = itemProduct.ProductNameTh;
                                            ProductList.Add(pro);
                                        }
                                        #endregion
                                        CriteriaModel.ProductLists = ProductList;
                                        CriteriaList.Add(CriteriaModel);
                                    }
                                    #endregion
                                    CategoryList.Add(CategoryModel);
                                    CategoryModel.CriteriaLists = CriteriaList;
                                }
                                #endregion

                                SchedulerModel.CategoryLists = CategoryList;
                                SchedulerList.Add(SchedulerModel);

                            }

                            #endregion

                            response.SchedulerLists = SchedulerList;
                            #endregion
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

        public CMSMasterResponse GetCMSMasterDetailById(int? CMSId)
        {
            CMSMasterResponse response = new CMSMasterResponse();
            try
            {

                if (CMSId != null && CMSId.HasValue)
                {
                    if (CMSId == 0)
                        return response;
                    using (ColspEntities db = new ColspEntities())
                    {
                        var GetCMS = db.CMSMasters.Where(c => c.CMSMasterId == CMSId).FirstOrDefault();
                        if (GetCMS != null)
                        {
                            response.CMSMasterId = GetCMS.CMSMasterId;
                            string CreateBy = string.Empty;
                            if (GetCMS.CreateBy.HasValue)
                            {
                                var by = db.Users.Where(c => c.UserId == GetCMS.CreateBy).FirstOrDefault();
                                if (by != null)
                                    CreateBy = by.NameEn;
                            }
                            response.CreateBy = CreateBy;
                            response.CMSMasterNameEN = GetCMS.CMSMasterNameEN;
                            response.CMSMasterNameTH = GetCMS.CMSMasterNameTH;
                            string CMSType = string.Empty;
                            if (GetCMS.CreateBy.HasValue)
                            {
                                var Type = db.CMSMasterTypes.Where(c => c.CMSMasterTypeId == GetCMS.CMSTypeId).FirstOrDefault();
                                if (Type != null)
                                    CMSType = Type.CMSMasterTypeNameEN;
                            }
                            response.CMSType = CMSType;
                            response.CMSMasterEffectiveDate = GetCMS.CMSMasterEffectiveDate;
                            response.CMSMasterEffectiveTime = GetCMS.CMSMasterEffectiveTime;
                            response.CMSMasterExpiryDate = GetCMS.CMSMasterExpiryDate;
                            response.CMSMasterExpiryTime = GetCMS.CMSMasterExpiryTime;
                            response.CreateIP = GetCMS.CreateIP;
                            response.LongDescriptionEN = GetCMS.LongDescriptionEN;
                            response.LongDescriptionTH = GetCMS.LongDescriptionTH;
                            response.ShortDescriptionEN = GetCMS.ShortDescriptionEN;
                            response.ShortDescriptionTH = GetCMS.ShortDescriptionTH;
                            response.CMSMasterStatusId = GetCMS.CMSMasterStatusId;
                            string Status = string.Empty;
                            if (GetCMS.CMSMasterStatusId.HasValue)
                            {
                                var st = db.CMSMasterStatus.Where(c => c.CMSMasterStatusId == GetCMS.CMSMasterStatusId).FirstOrDefault();
                                if (st != null)
                                    Status = st.CMSMasterStatusNameEN;
                            }
                            response.CMSStatus = Status;
                            response.CMSMasterURLKey = GetCMS.CMSMasterURLKey;
                            response.Visibility = GetCMS.Visibility;
                            response.CreateDate = (DateTime)GetCMS.Createdate;
                            #region Collection


                            List<SchedulerListResponse> SchedulerList = new List<SchedulerListResponse>();
                            var SchedulerLists = (from map in db.CMSMasterSchedulerMaps.Where(c => c.CMSMasterId == CMSId)
                                                  from sch in db.CMSSchedulers.Where(c => c.CMSSchedulerId == map.CMSSchedulerId).DefaultIfEmpty()
                                                  select new
                                                  {
                                                      map.CMSMasterId,
                                                      map.CMSSchedulerId,
                                                      map.CMSMasterSchedulerMapId,
                                                      sch.EffectiveDate,
                                                      sch.EffectiveTime,
                                                      sch.ExpiryDate,
                                                      sch.ExpiryTime
                                                  });
                            #region Scheduler
                            foreach (var Scheduleritem in SchedulerLists)
                            {
                                SchedulerListResponse SchedulerModel = new SchedulerListResponse();
                                List<CategoryListResponse> CategoryList = new List<CategoryListResponse>();
                                var CategoryLists = (from map in db.CMSCategorySchedulerMaps.Where(m => m.CMSSchedulerId == Scheduleritem.CMSSchedulerId)
                                                     from cat in db.CMSCategories.Where(c => c.CMSCategoryId == map.CMSCategoryId).DefaultIfEmpty()
                                                     select new
                                                     {
                                                         CMSCategoryId = map.CMSCategoryId,
                                                         CategoryNameEN = cat.CMSCategoryNameEN,
                                                         CategoryNameTH = cat.CMSCategoryNameTH
                                                     });
                                SchedulerModel.CMSSchedulerId = (int)Scheduleritem.CMSSchedulerId;
                                SchedulerModel.EffectiveDate = Scheduleritem.EffectiveDate;
                                SchedulerModel.EffectiveTime = Scheduleritem.EffectiveTime;
                                SchedulerModel.ExpiryDate = Scheduleritem.ExpiryDate;
                                SchedulerModel.ExpiryTime = Scheduleritem.ExpiryTime;
                                #region Cateegory
                                foreach (var itemCat in CategoryLists)
                                {
                                    CategoryListResponse CategoryModel = new CategoryListResponse();
                                    CategoryModel.CMSMasterId = GetCMS.CMSMasterId;
                                    CategoryModel.CMSCategoryId = (int)itemCat.CMSCategoryId;
                                    CategoryModel.CategoryNameEN = itemCat.CategoryNameEN;
                                    CategoryModel.CategoryNameTH = itemCat.CategoryNameTH;
                                    List<CriteriaListResponse> CriteriaList = new List<CriteriaListResponse>();
                                    var CriteriaLists = (from crc in db.CMSCategoryCreteriaMaps.Where(c => c.CMSCategoryId == itemCat.CMSCategoryId)
                                                         from cr in db.CMSCreterias.Where(c => c.CMSCreteriaId == crc.CMSCreteriaId).DefaultIfEmpty()
                                                         select new
                                                         {
                                                             cr.CMSCreteriaId,
                                                             cr.CategoryId,
                                                             cr.Brand,
                                                             cr.MinPrice,
                                                             cr.MaxPrice

                                                         });
                                    #region Criteria
                                    foreach (var itemCriteria in CriteriaLists)
                                    {
                                        CriteriaListResponse CriteriaModel = new CriteriaListResponse();
                                        List<ProductListResponse> ProductList = new List<ProductListResponse>();
                                        var ProductLists = (from map in db.CMSCriteriaProductMaps.Where(m => m.CMSCriteriaId == itemCriteria.CMSCreteriaId)
                                                            from pro in db.Products.Where(c => c.Pid == map.ProductPID).DefaultIfEmpty()
                                                            select new
                                                            {
                                                                map.ProductPID,
                                                                pro.ProductNameEn,
                                                                pro.ProductNameTh,
                                                                pro.SalePrice
                                                            });
                                        CriteriaModel.Brand = itemCriteria.Brand;
                                        CriteriaModel.CategoryId = itemCriteria.CategoryId;
                                        CriteriaModel.MaxPrice = itemCriteria.MaxPrice;
                                        CriteriaModel.MinPrice = itemCriteria.MinPrice;
                                        //list
                                        #region Product
                                        foreach (var itemProduct in ProductLists)
                                        {
                                            ProductListResponse pro = new ProductListResponse();
                                            pro.ProductPId = itemProduct.ProductPID;
                                            pro.ProductNameEN = itemProduct.ProductNameEn;
                                            pro.ProductNameTH = itemProduct.ProductNameTh;
                                            ProductList.Add(pro);
                                        }
                                        #endregion
                                        CriteriaModel.ProductLists = ProductList;
                                        CriteriaList.Add(CriteriaModel);
                                    }
                                    #endregion
                                    CategoryList.Add(CategoryModel);
                                    CategoryModel.CriteriaLists = CriteriaList;
                                }
                                #endregion

                                SchedulerModel.CategoryLists = CategoryList;
                                SchedulerList.Add(SchedulerModel);

                            }

                            #endregion

                            response.SchedulerLists = SchedulerList;
                            #endregion
                            return response;
                        }
                        else
                        {
                            return response;
                        }
                    }
                }
                else
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                return response;
            }
        }
        #endregion

        #region Search

        #endregion


        #region Edit/Update 
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

        [Route("api/CMS/Master/Update")]
        [HttpPost]
        public HttpResponseMessage CMSMasterUpdate(CMSMasterRequest model)
        {
            try
            {
                int CMSId = 0;
                if (model != null)
                {

                    CMSProcess cms = new CMSProcess();
                    CMSId = cms.UpdateCMSMaster(model);
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


        [Route("api/CMS/Group/Update")]
        [HttpPost]
        public HttpResponseMessage CMSGroupUpdate(CMSGroupRequest model)
        {
            try
            {
                if (model != null)
                {
                    int CMSGroupId = 0;
                    CMSProcess cmsGroup = new CMSProcess();
                    CMSGroupId = cmsGroup.EditCMSGroup(model);
                    return GetCMSGroup(CMSGroupId);
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
