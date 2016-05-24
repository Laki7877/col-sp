using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using Colsp.Api.Helpers;
using Colsp.Logic;
using Colsp.Model;
using System.Dynamic;

namespace Colsp.Api.Controllers
{
    public class CMSController : ApiController
    {

        private ColspEntities db;
        private CMSLogic cmsLogic;

        int ShopId
        {
            get
            {
                int shopId = this.User.UserRequest().Type == "A" ? 0 : this.User.ShopRequest().ShopId;
                return shopId;
            }
        }

        public CMSController()
        {
            this.db = new ColspEntities();
            this.cmsLogic = new CMSLogic();
        }

        #region CMS Category

        // Get CMS Category List
        [HttpGet]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage GetAllCMSCategory([FromUri] CMSCategoryRequest request)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from category in db.CMSCategories where !category.Status.Equals("RM") select category;

                // Shop
                if (shopId != 0)
                {
                    var cmsCategoryIdList = (from map in db.CMSCategoryProductMaps where map.ShopId == shopId select map).Select(s => s.CMSCategoryId).ToList();
                    query = query.Where(x => cmsCategoryIdList.Contains(x.CMSCategoryId));
                }

                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    query = query.Where(x => x.CMSCategoryNameEN.Contains(request.SearchText) || x.CMSCategoryNameTH.Contains(request.SearchText));
                }

                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_WAIT_FOR_APPROVAL));
                    }
                }

                var total = query.Count();
                var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSCategory");
            }

        }

        // Get CMS Category Item
        [HttpGet]
        [Route("api/CMS/CMSCategory/{cmsCategoryId}")]
        public HttpResponseMessage GetCMSCategory([FromUri] int cmsCategoryId)
        {
            try
            {
                var query = from cate in db.CMSCategories
                            where cate.CMSCategoryId == cmsCategoryId
                            select new CMSCategoryRequest
                            {
                                CategoryProductList = (from pMap in db.CMSCategoryProductMaps
                                                       where pMap.CMSCategoryId == cate.CMSCategoryId
                                                       select new CMSCategoryProductMapRequest
                                                       {
                                                           CMSCategoryProductMapId = pMap.CMSCategoryProductMapId,
                                                           CMSCategoryId = pMap.CMSCategoryId,
                                                           Pid = pMap.Pid,
                                                           ProductBoxBadge = pMap.ProductBoxBadge,
                                                           Sequence = pMap.Sequence,
                                                           FeatureImgUrl = (from p in db.Products
                                                                            where p.Pid.Equals(pMap.Pid)
                                                                            select p).FirstOrDefault().FeatureImgUrl,
                                                           OriginalPrice = (from p in db.Products
                                                                            where p.Pid.Equals(pMap.Pid)
                                                                            select p).FirstOrDefault().OriginalPrice,
                                                           ExpireDate = (from p in db.Products
                                                                         where p.Pid.Equals(pMap.Pid)
                                                                         select p).FirstOrDefault().ExpireDate

                                                       }).ToList(),


                                CMSCategoryId = cate.CMSCategoryId,
                                CMSCategoryNameEN = cate.CMSCategoryNameEN,
                                CMSCategoryNameTH = cate.CMSCategoryNameTH,
                                Visibility = cate.Visibility,
                                Status = cate.Status,
                                UpdateOn = cate.UpdateOn
                            };

                if (!query.Any())
                    Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Category");

                var item = query.FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, item);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSCategory");
            }

        }

        // Get Brand List
        [HttpGet]
        [Route("api/CMS/GetBrand/{categoryId}")]
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

                var items = query.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, items);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetBrand");
            }

        }

        // Get Tag List
        [HttpGet]
        [Route("api/CMS/GetAllTag")]
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

        // Get Category List
        [HttpGet]
        [Route("api/CMS/GetAllCategory")]
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
        [Route("api/CMS/SearchProduct")]
        public HttpResponseMessage SearchProduct([FromUri] ProductCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from product in db.Products select product;

                if (shopId != 0)
                    query = query.Where(x => x.ShopId == shopId);

                if (condition.SearchBy == SearchOption.PID)
                    query = query.Where(x => x.Pid.Equals(condition.SearchText));

                if (condition.SearchBy == SearchOption.SKU)
                    query = query.Where(x => x.Sku.Equals(condition.SearchText));

                if (condition.SearchBy == SearchOption.ProductName)
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
        [Route("api/CMS/SearchFeatureProduct")]
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

        [HttpGet]
        [Route("api/CMS/SearchCMSCategory")]
        public HttpResponseMessage SearchCMSCategory([FromUri] BaseCondition condition)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from cate in db.CMSCategories select cate;

                if (shopId != 0)
                {
                    var cmsCategoryIds = (from map in db.CMSCategoryProductMaps where map.ShopId == shopId select map).Select(x => x.CMSCategoryId).ToList();
                    query = query.Where(x => cmsCategoryIds.Contains(x.CMSCategoryId));
                }

                if (condition.SearchText != null)
                    query = query.Where(x =>
                            x.CMSCategoryNameEN.Contains(condition.SearchText) ||
                            x.CMSCategoryNameTH.Contains(condition.SearchText));

                query = query.Take(100);

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found CMS Category.");

                List<CMSCategoryRequest> categories = new List<CMSCategoryRequest>();

                foreach (var c in query)
                {
                    CMSCategoryRequest item = new CMSCategoryRequest();
                    item.CMSCategoryId = c.CMSCategoryId;
                    item.CMSCategoryNameEN = c.CMSCategoryNameEN;
                    item.CMSCategoryNameTH = c.CMSCategoryNameTH;
                    item.Visibility = c.Visibility;
                    item.Total = (from pm in db.CMSCategoryProductMaps where pm.CMSCategoryId == c.CMSCategoryId select pm).Count();

                    categories.Add(item);
                }

                return Request.CreateResponse(HttpStatusCode.OK, categories);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/SearchCMSCategory");
            }
        }

        // Visibility CMS Category
        [Route("api/CMS/CMSCategory/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCMSCategory(List<CMSCategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var cateList = db.CMSCategories.ToList();
                if (cateList == null || cateList.Count == 0)
                {
                    throw new Exception("Not found cms category");
                }
                foreach (CMSCategoryRequest cateRq in request)
                {

                    var current = cateList.Where(w => w.CMSCategoryId == cateRq.CMSCategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find cms category " + cateRq.CMSCategoryId);
                    }

                    current.Visibility = cateRq.Visibility;

                }
                Util.DeadlockRetry(db.SaveChanges, "CMSCategory");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        // Save CMS Category
        [HttpPost]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage SaveCMSCategory(CMSCategoryRequest request)
        {
            try
            {
                request.Status = Constant.CMS_STATUS_WAIT_FOR_APPROVAL;

                var success = cmsLogic.AddCMSCategory(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/AddCMSCategory");
            }

        }

        // Edit CMS Category
        [HttpPut]
        [Route("api/CMS/CMSCategory/{cmsCategoryId}")]
        public HttpResponseMessage EditCMSCategory([FromUri] int cmsCategoryId, CMSCategoryRequest request)
        {
            try
            {
                var success = cmsLogic.EditCMSCategory(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/EditCMSCategory");
            }

        }

        // Delete CMS Category
        [HttpDelete]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage DeleteCMSCategory(List<CMSCategoryRequest> request)
        {
            try
            {
                var success = cmsLogic.DeleteCMSCategory(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/DeleteCMSCategory");
            }

        }

        // Approve CMS Category
        [Route("api/CMS/CMSCategory/Approve")]
        [HttpPut]
        public HttpResponseMessage ApproveCMSCategory(List<CMSCategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var cmsCategoryIds = request.Select(s => s.CMSCategoryId).ToList();
                var groupList = db.CMSCategories.Where(w => cmsCategoryIds.Contains(w.CMSCategoryId)).ToList();
                foreach (CMSCategoryRequest cateRq in request)
                {
                    var cate = groupList.Where(w => w.CMSCategoryId == cateRq.CMSCategoryId).SingleOrDefault();
                    if (cate == null)
                    {
                        throw new Exception("Cannot find deleted cms category");
                    }

                    cate.Status = Constant.CMS_STATUS_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "CMSCategory");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        // Reject CMS Category
        [Route("api/CMS/CMSCategory/Reject")]
        [HttpPut]
        public HttpResponseMessage RejectCMSCategory(List<CMSCategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var cmsCategoryIds = request.Select(s => s.CMSCategoryId).ToList();
                var groupList = db.CMSCategories.Where(w => cmsCategoryIds.Contains(w.CMSCategoryId)).ToList();
                foreach (CMSCategoryRequest cateRq in request)
                {
                    var cate = groupList.Where(w => w.CMSCategoryId == cateRq.CMSCategoryId).SingleOrDefault();
                    if (cate == null)
                    {
                        throw new Exception("Cannot find deleted cms category");
                    }

                    cate.Status = Constant.CMS_STATUS_NOT_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "CMSMaster");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region CMS Master

        // Get CMS Master List
        [HttpGet]
        [Route("api/CMS/CMSMaster")]
        public HttpResponseMessage GetAllCMSMaster([FromUri] CMSMasterRequest request)
        {
            try
            {
                var shopId = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;

                var query = from master in db.CMSMasters where !master.Status.Equals("RM") select master;

                if (!string.IsNullOrEmpty(request.SearchText))
                    query = query.Where(x => x.CMSMasterNameEN.Contains(request.SearchText) || x.CMSMasterNameTH.Contains(request.SearchText));

                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(x => x.Status.Equals(Constant.CMS_STATUS_WAIT_FOR_APPROVAL));
                    }
                }

                var total = query.Count();
                var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSCategory");
            }

        }

        // Get CMS Master Item
        [HttpGet]
        [Route("api/CMS/CMSMaster/{cmsMasterId}")]
        public HttpResponseMessage GetCMSMaster([FromUri] int cmsMasterId)
        {
            try
            {

                var query = from master in db.CMSMasters
                            join masterScheduleMap in db.CMSMasterSchedulerMaps
                            on master.CMSMasterId equals masterScheduleMap.CMSMasterId
                            where master.CMSMasterId == cmsMasterId
                            select new CMSMasterRequest
                            {
                                CMSMasterId                 = master.CMSMasterId,
                                CMSMasterNameEN             = master.CMSMasterNameEN,
                                CMSMasterNameTH             = master.CMSMasterNameTH,
                                CMSMasterType               = master.CMSMasterType,
                                CMSMasterURLKey             = master.CMSMasterURLKey,
                                EffectiveDate               = master.CMSMasterEffectiveDate,
                                ExpiryDate                  = master.CMSMasterExpireDate,
                                LongDescriptionEN           = master.LongDescriptionEN,
                                LongDescriptionTH           = master.LongDescriptionTH,
                                ShortDescriptionEN          = master.ShortDescriptionEN,
                                ShortDescriptionTH          = master.ShortDescriptionTH,
                                MobileLongDescriptionEN     = master.MobileLongDescriptionEN,
                                MobileLongDescriptionTH     = master.MobileLongDescriptionTH,
                                MobileShortDescriptionEN    = master.MobileShortDescriptionEN,
                                MobileShortDescriptionTH    = master.MobileShortDescriptionTH,
                                Status                      = master.Status,
                                Visibility                  = master.Visibility,
                                ISCampaign                  = master.IsCampaign,
                                FeatureTitle                = master.FeatureTitle,
                                TitleShowcase               = master.TitleShowcase,

                                FeatureProductList          = (from feature in db.CMSFeatureProducts
                                                              where feature.CMSMasterId == cmsMasterId
                                                              select new CMSFeatureProductRequest {
                                                                  CMSMasterId = feature.CMSMasterId,
                                                                  ProductId = feature.ProductId
                                                              }).ToList(),

                                ScheduleList                = (from schedule in db.CMSSchedulers
                                                                where schedule.CMSSchedulerId == masterScheduleMap.CMSSchedulerId
                                                                select new CMSSchedulerRequest
                                                                {
                                                                    CMSSchedulerId = schedule.CMSSchedulerId,
                                                                    CMSMasterId = master.CMSMasterId,
                                                                    EffectiveDate = schedule.EffectiveDate,
                                                                    ExpiryDate = schedule.ExpireDate
                                                                }).ToList(),

                                CategoryList                = (from cate in db.CMSCategories
                                                                join masterCate in db.CMSMasterCategoryMaps 
                                                                on cate.CMSCategoryId equals masterCate.CMSCategoryId 
                                                                where masterCate.CMSMasterId == cmsMasterId 
                                                                select new CMSCategoryRequest
                                                                {
                                                                    CMSCategoryId = cate.CMSCategoryId,
                                                                    CMSCategoryNameEN = cate.CMSCategoryNameEN,
                                                                    CMSCategoryNameTH = cate.CMSCategoryNameTH,
                                                                    Visibility = cate.Visibility,
                                                                    Status = cate.Status,
                                                                    Total = (from pm in db.CMSCategoryProductMaps where pm.CMSCategoryId == cate.CMSCategoryId select pm).Count()
                                                                }).ToList()
                            };

                if (!query.Any())
                    Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Category");

                var item = query.FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, item);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSCategory");
            }

        }

        [Route("api/CMS/CMSImage")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                //FileUploadRespond fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.CMS_FOLDER, 1500, 1500, 2000, 2000, 5, true);
                var fileUpload = 0;
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        // Save CMS Master
        [HttpPost]
        [Route("api/CMS/CMSMaster")]
        public HttpResponseMessage SaveCMSMaster(CMSMasterRequest request)
        {
            try
            {
                var ShopId = this.User.UserRequest().IsAdmin ? 0 : this.User.ShopRequest().ShopId;
                var Email  = this.User.UserRequest().Email;

                request.Status      = Constant.CMS_STATUS_WAIT_FOR_APPROVAL;
                request.CreateBy    = Email;
                
                var success = cmsLogic.AddCMSMaster(request);

                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/AddCMSMaster");
            }

        }

        // Edit CMS Master
        [HttpPut]
        [Route("api/CMS/CMSMaster/{cmsMasterId}")]
        public HttpResponseMessage EditCMSMaster([FromUri] int cmsMasterId, CMSMasterRequest request)
        {
            try
            {
                var ShopId  = this.User.UserRequest().IsAdmin ? 0 : this.User.ShopRequest().ShopId;
                var Email   = this.User.UserRequest().Email;

                var success = cmsLogic.EditCMSMaster(request, ShopId, Email);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/EditCMSMaster");
            }

        }

        // Delete CMS Category
        [HttpDelete]
        [Route("api/CMS/CMSMaster")]
        public HttpResponseMessage DeleteCMSMaster(List<CMSMasterRequest> request)
        {
            try
            {
                var success = cmsLogic.DeleteCMSMaster(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/DeleteCMSCategory");
            }

        }

        // Visibility CMS Master
        [Route("api/CMS/CMSMaster/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCMSMaster(List<CMSMasterRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var masterList = db.CMSMasters.ToList();
                if (masterList == null || masterList.Count == 0)
                {
                    throw new Exception("Not found cms master");
                }
                foreach (CMSMasterRequest masterRq in request)
                {

                    var current = masterList.Where(w => w.CMSMasterId == masterRq.CMSMasterId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find cms master " + masterRq.CMSMasterId);
                    }

                    current.Visibility = masterRq.Visibility;

                }
                Util.DeadlockRetry(db.SaveChanges, "CMSMaster");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        // Approve CMS Master
        [Route("api/CMS/CMSMaster/Approve")]
        [HttpPut]
        public HttpResponseMessage ApproveCMSMaster(List<CMSMasterRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var cmsMasterIds = request.Select(s => s.CMSMasterId).ToList();
                var groupList = db.CMSMasters.Where(w => cmsMasterIds.Contains(w.CMSMasterId)).ToList();
                foreach (CMSMasterRequest masterRq in request)
                {
                    var master = groupList.Where(w => w.CMSMasterId == masterRq.CMSMasterId).SingleOrDefault();
                    if (master == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }

                    master.Status = Constant.CMS_STATUS_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "CMSMaster");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        // Reject CMS Master
        [Route("api/CMS/CMSMaster/Reject")]
        [HttpPut]
        public HttpResponseMessage RejectCMSMaster(List<CMSMasterRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var cmsMasterIds = request.Select(s => s.CMSMasterId).ToList();
                var groupList = db.CMSMasters.Where(w => cmsMasterIds.Contains(w.CMSMasterId)).ToList();
                foreach (CMSMasterRequest masterRq in request)
                {
                    var master = groupList.Where(w => w.CMSMasterId == masterRq.CMSMasterId).SingleOrDefault();
                    if (master == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }

                    master.Status = Constant.CMS_STATUS_NOT_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "CMSMaster");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        #endregion

        #region CMS Group

        #region Get Method
        // Get CMS Group List
        [HttpGet]
        [Route("api/CMS/CMSGroup")]
        public HttpResponseMessage GetAllCMSGroup([FromUri] CMSGroupRequest request)
        {
            try
            {
                int shopId = 0;

                var query = from g in db.CMSGroups where !g.Status.Equals("RM") select g;

                var cmsGroupIdList = (from map in db.CMSMasterGroupMaps where map.ShopId == shopId select map).Select(s => s.CMSGroupId).ToList();

                if (cmsGroupIdList.Count > 0)
                    query = query.Where(x => cmsGroupIdList.Contains(x.CMSGroupId));

                if (!string.IsNullOrEmpty(request.SearchText))
                    query = query.Where(x => x.CMSGroupNameEN.Contains(request.SearchText) || x.CMSGroupNameTH.Contains(request.SearchText));

                var total = query.Count();
                var response = PaginatedResponse.CreateResponse(query.Paginate(request), request, total);

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSGroup");
            }

        }

        // Get CMS Group Item
        [HttpGet]
        [Route("api/CMS/CMSGroup/{cmsGroupId}")]
        public HttpResponseMessage GetCMSGroup([FromUri] int cmsGroupId)
        {
            try
            {
                var query = from g in db.CMSGroups
                            where g.CMSGroupId == cmsGroupId
                            select new CMSGroupRequest
                            {
                                GroupMasterList = (from masterGroup in db.CMSMasterGroupMaps
                                                   where masterGroup.CMSGroupId == g.CMSGroupId
                                                   select new CMSMasterGroupMapRequest
                                                   {
                                                       CMSMasterGroupMapId = masterGroup.CMSMasterGroupMapId,
                                                       CMSGroupId = masterGroup.CMSGroupId,
                                                       CMSMasterId = masterGroup.CMSMasterId,
                                                       Sequence = masterGroup.Sequence,
                                                       Status = masterGroup.Status,
                                                       CMSMasterExpiryDate = (from p in db.CMSMasters
                                                                              where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                              select p).FirstOrDefault().CMSMasterExpireDate,

                                                       CMSMasterNameEN = (from p in db.CMSMasters
                                                                          where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                          select p).FirstOrDefault().CMSMasterNameEN,

                                                       CMSMasterNameTH = (from p in db.CMSMasters
                                                                          where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                          select p).FirstOrDefault().CMSMasterNameTH,
                                                   }).ToList(),

                                CMSGroupId = g.CMSGroupId,
                                CMSGroupNameEN = g.CMSGroupNameEN,
                                CMSGroupNameTH = g.CMSGroupNameTH,
                                Status = g.Status,
                                Visibility = g.Visibility,
                                UpdateOn = g.UpdateOn
                            };

                if (!query.Any())
                    Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Group");

                var item = query.FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, item);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/GetAllCMSGroup");
            }

        }

        // Search CMS Master
        [HttpGet]
        [Route("api/CMS/SearchCMSMaster")]
        public HttpResponseMessage SearchCMSMaster([FromUri] CMSMasterCondition condition)
        {
            try
            {
                int shopId = 0; //this.User.ShopRequest().ShopId;

                var query = from master in db.CMSMasters select master;

                if (condition.SearchText != null)
                    query = query.Where(x =>
                            x.CMSMasterNameEN.Contains(condition.SearchText) ||
                            x.CMSMasterNameTH.Contains(condition.SearchText));

                query = query.Take(100);

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found CMS Master.");

                List<CMSMaster> masters = new List<CMSMaster>();

                foreach (var m in query)
                {
                    CMSMaster item = new CMSMaster();
                    item.CMSMasterId = m.CMSMasterId;
                    item.CMSMasterNameEN = m.CMSMasterNameEN;
                    item.CMSMasterNameTH = m.CMSMasterNameTH;
                    //item.Status                 = m.CMSMasterStatusId;
                    item.CMSMasterEffectiveDate = m.CMSMasterEffectiveDate;
                    item.CMSMasterExpireDate    = m.CMSMasterExpireDate;
                    masters.Add(item);
                }

                return Request.CreateResponse(HttpStatusCode.OK, masters);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/SearchCMSMaster");
            }

        }
        #endregion

        #region Post Method

        // Save CMS Group
        [HttpPost]
        [Route("api/CMS/CMSGroup")]
        public HttpResponseMessage SaveCMSGroup(CMSGroupRequest request)
        {
            try
            {
                var success = cmsLogic.AddCMSGroup(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/AddCMSGroup");
            }

        }
        #endregion

        #region Put Group
        // Edit CMS Group
        [HttpPut]
        [Route("api/CMS/CMSGroup/{cmsGroupId}")]
        public HttpResponseMessage EditCMSGroup([FromUri] int cmsGroupId, CMSGroupRequest request)
        {
            try
            {
                var success = cmsLogic.EditCMSGroup(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/EditCMSGroup");
            }

        }

        // Visibility CMS Group
        [Route("api/CMS/CMSGroup/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCMSGroup(List<CMSGroupRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var groupList = db.CMSGroups.ToList();
                if (groupList == null || groupList.Count == 0)
                {
                    throw new Exception("Not found cms group");
                }
                foreach (CMSGroupRequest groupRq in request)
                {

                    var current = groupList.Where(w => w.CMSGroupId == groupRq.CMSGroupId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find cms group " + groupRq.CMSGroupId);
                    }

                    current.Visibility = groupRq.Visibility;

                }
                Util.DeadlockRetry(db.SaveChanges, "CMSGroup");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
        #endregion

        #region Delete Method

        // Delete CMS Group
        [HttpDelete]
        [Route("api/CMS/CMSGroup")]
        public HttpResponseMessage DeleteCMSGroup(List<CMSGroupRequest> request)
        {
            try
            {
                var success = cmsLogic.DeleteCMSGroup(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/DeleteCMSGroup");
            }

        }
        #endregion

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
