﻿using System;
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

        public CMSController()
        {
            this.db = new ColspEntities();
            this.cmsLogic = new CMSLogic();
        }

        #region Get Method

        // Get CMS Category List
        [HttpGet]
        [Route("api/CMS/CMSCategory")]
        public HttpResponseMessage GetAllCMSCategory([FromUri] CMSCategoryRequest request)
        {
            try
            {
                int shopId = 0;

                var query = from category in db.CMSCategories select category;

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
                                
                                CMSCategoryId = cate.CMSCategoryId,
                                CMSCategoryNameEN = cate.CMSCategoryNameEN,
                                CMSCategoryNameTH = cate.CMSCategoryNameTH,
                                IsActive = cate.IsActive,
                                UpdateDate = cate.UpdateDate
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
        public HttpResponseMessage GetAllCategory()
        {
            try
            {
                int shopId = 0; //this.User.ShopRequest().ShopId;

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


                if (!query.Any())
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Not Found Brand");

                var items = query.ToList();
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
                throw new Exception(ex.Message + " /api/CMS/AddCMSCategory");
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
