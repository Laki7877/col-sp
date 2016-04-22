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

        #region Get Method

        // Get CMS Category List
        [HttpGet]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage GetAllCMSCategory([FromUri] CMSCategoryRequest request)
        {
            try
            {
                int shopId = 0;

                var query = from category in db.CMSCategories where !category.Status.Equals("RM") select category;

                if (shopId != 0)
                {
                    var cmsCategoryIdList = (from map in db.CMSCategoryProductMaps where map.ShopId == shopId select map).Select(s => s.CMSCategoryId).ToList();
                    query = query.Where(x => cmsCategoryIdList.Contains(x.CMSCategoryId));
                }

                if (!string.IsNullOrEmpty(request.SearchText))
                    query = query.Where(x => x.CMSCategoryNameEN.Contains(request.SearchText) || x.CMSCategoryNameTH.Contains(request.SearchText));

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
                                
                                CMSCategoryId       = cate.CMSCategoryId,
                                CMSCategoryNameEN   = cate.CMSCategoryNameEN,
                                CMSCategoryNameTH   = cate.CMSCategoryNameTH,
                                Visibility          = cate.Visibility.Value,
                                Status              = cate.Status,
                                UpdateDate          = cate.UpdateDate
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
                int shopId = 0; //this.User.ShopRequest().ShopId;

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

                // mockup
                //List<BrandRequest> brands = new List<BrandRequest>();
                //brands.Add(new BrandRequest
                //{
                //    BrandId = 1,
                //    BrandNameEn = "Test Brand EN",
                //    BrandNameTh = "Test Brand TH"
                //});

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
                // int shopId = this.User.UserRequest().Type == "A" ? 0 : this.User.ShopRequest().ShopId;

                var query = this.ShopId == 0
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
                int shopId = 0; //this.User.ShopRequest().ShopId;

                var query = from product in db.Products select product;
                
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
                    item.ExpireDate    = p.ExpireDate;
                    item.OriginalPrice = p.OriginalPrice;
                    item.Sku           = p.Sku;
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
                int shopId = 0;

                var query = from cate in db.CMSCategories select cate;

                if (condition.SearchText != null)
                    query = query.Where(x =>
                            x.CMSCategoryNameEN.Contains(condition.SearchText) ||
                            x.CMSCategoryNameTH.Contains(condition.SearchText));

                query = query.Take(100);

                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found CMS Category.");

                List<CMSCategory> categories = new List<CMSCategory>();

                foreach (var c in query)
                {
                    CMSCategory item = new CMSCategory();
                    item.CMSCategoryId = c.CMSCategoryId;
                    item.CMSCategoryNameEN = c.CMSCategoryNameEN;
                    item.CMSCategoryNameTH = c.CMSCategoryNameTH;
                    item.Visibility = c.Visibility;
                    item.IsCampaign = c.IsCampaign;
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
        #endregion

        #region Post Method

        // Save CMS Category
        [HttpPost]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage SaveCMSCategory(CMSCategoryRequest request)
        {
            try
            {
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
        #endregion

        #region Put Method
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
        
        #endregion

        #region Delete Method

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
        #endregion

        #endregion

        #region CMS Master

        // Get CMS Master List
        [HttpGet]
        [Route("api/CMS/CMSMaster")]
        public HttpResponseMessage GetAllCMSMaster([FromUri] CMSMasterRequest request)
        {
            try
            {
                int shopId = 0;

                var query = from master in db.CMSMasters
                select new CMSMasterRequest
                {
                    CMSMasterId         = master.CMSMasterId,
                    CMSMasterNameEN     = master.CMSMasterNameEN,
                    CMSMasterNameTH     = master.CMSMasterNameTH,
                    CMSMasterURLKey     = master.CMSMasterURLKey,
                    CMSMasterType       = master.CMSMasterType,
                    CreateBy            = master.CreateBy,
                    UpdateBy            = master.UpdateBy,
                    UpdateDate          = master.UpdateDate,
                    Status              = master.Status,
                    Visibility          = master.Visibility.Value
                };

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

                request._order = "CMSMasterId";
                
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
                                CMSMasterId         = master.CMSMasterId,
                                CMSMasterNameEN     = master.CMSMasterNameEN,
                                CMSMasterNameTH     = master.CMSMasterNameTH,
                                CMSMasterStatusId   = master.CMSMasterStatusId,
                                CMSMasterType       = master.CMSMasterType,
                                CMSMasterURLKey     = master.CMSMasterURLKey,
                                EffectiveDate       = master.CMSMasterEffectiveDate,
                                ExpiryDate          = master.CMSMasterExpiryDate,
                                LongDescriptionEN   = master.LongDescriptionEN,
                                LongDescriptionTH   = master.LongDescriptionTH,
                                ShortDescriptionEN  = master.ShortDescriptionEN,
                                ShortDescriptionTH  = master.ShortDescriptionTH,
                                MobileLongDescriptionEN = master.MobileLongDescriptionEN,
                                MobileLongDescriptionTH = master.MobileLongDescriptionTH,
                                MobileShortDescriptionEN = master.MobileShortDescriptionEN,
                                MobileShortDescriptionTH = master.MobileShortDescriptionTH,
                                Status              = master.Status,
                                Visibility          = master.Visibility.Value,
                                ISCampaign          = master.IsCampaign.Value,
                                ScheduleList        = (from schedule in db.CMSSchedulers 
                                                        where schedule.CMSSchedulerId == masterScheduleMap.CMSSchedulerId
                                                        select new CMSSchedulerRequest
                                                        {
                                                            CMSSchedulerId  = schedule.CMSSchedulerId,
                                                            CMSMasterId     = master.CMSMasterId,
                                                            EffectiveDate   = schedule.EffectiveDate,
                                                            ExpiryDate      = schedule.ExpiryDate,
                                                            CategoryList    = (from cate in db.CMSCategories
                                                                            join cateScheduleMap in db.CMSCategorySchedulerMaps
                                                                            on cate.CMSCategoryId equals cateScheduleMap.CMSCategoryId
                                                                            where cateScheduleMap.CMSSchedulerId == schedule.CMSSchedulerId
                                                                            select new CMSCategoryRequest
                                                                            {
                                                                                CMSCategoryId       = cate.CMSCategoryId,
                                                                                CMSCategoryNameEN   = cate.CMSCategoryNameEN,
                                                                                CMSCategoryNameTH   = cate.CMSCategoryNameTH,
                                                                                Visibility          = cate.Visibility.Value,
                                                                                Status              = cate.Status
                                                                            }).ToList()
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

        // Save CMS Master
        [HttpPost]
        [Route("api/CMS/CMSMaster")]
        public HttpResponseMessage SaveCMSMaster(CMSMasterRequest request)
        {
            try
            {
                // set default
                request.Status = Constant.CMS_STATUS_WAIT_FOR_APPROVAL;

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
                var success = cmsLogic.EditCMSMaster(request);
                if (!success)
                    Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");

                return Request.CreateResponse(HttpStatusCode.OK, success);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " /api/CMS/EditCMSMaster");
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
                                                            CMSMasterGroupMapId  = masterGroup.CMSMasterGroupMapId,
                                                            CMSGroupId           = masterGroup.CMSGroupId,
                                                            CMSMasterId          = masterGroup.CMSMasterId,
                                                            Sequence             = masterGroup.Sequence,
                                                            IsActive             = masterGroup.IsActive,
                                                            CMSMasterExpiryDate  = (from p in db.CMSMasters
                                                                                        where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                                        select p).FirstOrDefault().CMSMasterExpiryDate,

                                                            CMSMasterNameEN      = (from p in db.CMSMasters
                                                                                        where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                                        select p).FirstOrDefault().CMSMasterNameEN,

                                                            CMSMasterNameTH     = (from p in db.CMSMasters
                                                                                    where p.CMSMasterId.Equals(masterGroup.CMSMasterId)
                                                                                    select p).FirstOrDefault().CMSMasterNameTH,
                                                       }).ToList(),

                                CMSGroupId      = g.CMSGroupId,
                                CMSGroupNameEN  = g.CMSGroupNameEN,
                                CMSGroupNameTH  = g.CMSGroupNameTH,
                                Status          = g.Status,
                                Visibility      = g.Visibility.Value,
                                UpdateDate      = g.UpdateDate
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
                    CMSMaster item              = new CMSMaster();
                    item.CMSMasterId            = m.CMSMasterId;
                    item.CMSMasterNameEN        = m.CMSMasterNameEN;
                    item.CMSMasterNameTH        = m.CMSMasterNameTH;
                    item.CMSMasterStatusId      = m.CMSMasterStatusId;
                    item.CMSMasterEffectiveDate = m.CMSMasterEffectiveDate;
                    item.CMSMasterExpiryDate    = m.CMSMasterExpiryDate;
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
