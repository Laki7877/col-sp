using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using System.Data.SqlClient;

namespace Colsp.Api.Controllers
{
    public class LocalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/LocalCategories")]
        [HttpGet]
        public HttpResponseMessage GetCategoryFromShop()
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var localCat = (from cat in db.LocalCategories
                                where cat.ShopId == shopId
                                select new
                                {
                                    cat.CategoryId,
                                    cat.NameEn,
                                    cat.NameTh,
                                    cat.Lft,
                                    cat.Rgt,
                                    cat.UrlKeyEn,
                                    cat.UrlKeyTh,
                                    cat.Visibility,
                                    cat.Status,
                                    cat.UpdatedDt,
                                    cat.CreatedDt,
                                    ProductCount = cat.ProductStages.Count,
                                });
                return Request.CreateResponse(HttpStatusCode.OK, localCat);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories/{categoryId}/ProductStages")]
        [HttpGet]
        public HttpResponseMessage GetProductStage([FromUri] ProductRequest request)
        {
            try
            {
                request.DefaultOnNull();
                IQueryable<ProductStage> products = null;
                products = db.ProductStages.Where(p => true);
                if (request.SearchText != null)
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText));
                }
                if (request.CategoryId != null)
                {
                    products = products.Where(p => p.LocalCatId == request.CategoryId);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Category is invalid");
                }
                var total = products.Count();
                var pagedProducts = products.GroupJoin(db.ProductStageImages,
                                                p => p.Pid,
                                                m => m.Pid,
                                                (p, m) => new
                                                {
                                                    p.Sku,
                                                    p.ProductId,
                                                    p.ProductNameEn,
                                                    p.ProductNameTh,
                                                    p.OriginalPrice,
                                                    p.SalePrice,
                                                    p.Status,
                                                    p.ImageFlag,
                                                    p.InfoFlag,
                                                    Modified = p.UpdatedDt,
                                                    ImageUrl = m.Where(w=>w.FeatureFlag == true).FirstOrDefault().ImageUrlEn,
                                                }
                                            )
                                            .Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/LocalCategories/{categoryId}")]
        [HttpGet]
        public HttpResponseMessage GetLocalCategory([FromUri] int categoryId)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var cat = db.LocalCategories.Where(w => w.ShopId == shopId && w.CategoryId == categoryId).SingleOrDefault();
                if(cat == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, cat);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/LocalCategories")]
        [HttpPost]
        public HttpResponseMessage AddGlobalCategory(CategoryRequest request)
        {
            LocalCategory category = null;
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                category = new LocalCategory();
                category.NameEn = request.NameEn;
                category.NameTh = request.NameTh;
                category.ShopId = shopId;
                category.UrlKeyEn = "";
                

                category.Visibility = request.Visibility;
                category.Status = request.Status;
                category.CreatedBy = this.User.UserRequest().Email;
                category.CreatedDt = DateTime.Now;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                
                int? max = db.LocalCategories.Where(w=>w.ShopId==shopId).Max(m => m.Rgt);
                if (max == null)
                {
                    category.Lft = 1;
                    category.Rgt = 2;
                }
                else
                {
                    category.Lft = max.Value + 1;
                    category.Rgt = max.Value + 2;
                }
                db.LocalCategories.Add(category);
                db.SaveChanges();
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Replace(" ", "-");
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, category);
            }
            catch (DbUpdateException e)
            {
                if (category != null && category.CategoryId != 0)
                {
                    db.LocalCategories.Remove(category);
                    db.SaveChanges();
                }
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                if (category != null && category.CategoryId != 0)
                {
                    db.LocalCategories.Remove(category);
                    db.SaveChanges();
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/LocalCategories/{categoryId}")]
        [HttpPut]
        public HttpResponseMessage SaveChange(int categoryId, CategoryRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var category = db.LocalCategories.Where(w => w.CategoryId == categoryId && w.ShopId== shopId).SingleOrDefault();
                if (category == null)
                {
                    throw new Exception("Cannot find selected category");
                }
                category.NameEn = request.NameEn;
                category.NameTh = request.NameTh;
                if (string.IsNullOrWhiteSpace(request.UrlKeyEn))
                {
                    category.UrlKeyEn = string.Concat(category.NameEn.Replace(" ", "-"), "-", category.CategoryId);
                }
                else
                {
                    category.UrlKeyEn = request.UrlKeyEn.Replace(" ", "-");
                }
                category.Visibility = request.Visibility;
                category.Status = request.Status;
                category.UpdatedBy = this.User.UserRequest().Email;
                category.UpdatedDt = DateTime.Now;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, category);
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/LocalCategories")]
        [HttpPut]
        public HttpResponseMessage SaveChangeLocalCategory(List<CategoryRequest> request)
        {
            try
            {
                int shopIdRq = this.User.ShopRequest().ShopId.Value;
                if (shopIdRq == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Shop is invalid. Cannot find shop in session");
                }
                var catEnList = (from cat in db.LocalCategories
                                 join proStg in db.ProductStages on cat.CategoryId equals proStg.LocalCatId into j
                                 from j2 in j.DefaultIfEmpty()
                                 where cat.ShopId == shopIdRq
                                 group j2 by cat into g
                                 select new
                                 {
                                     Category = g,
                                     ProductCount = g.Key.ProductStages.Count
                                 }).ToList();
                foreach (CategoryRequest catRq in request)
                {
                    if (catRq.Lft == null || catRq.Rgt == null || catRq.Lft >= catRq.Rgt)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node is not properly formated");
                    }
                    var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
                    if (validate != null && validate.Count > 1)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node child has duplicated left or right key");
                    }
                    if (catRq.CategoryId != 0)
                    {
                        var catEn = catEnList.Where(w => w.Category.Key.CategoryId == catRq.CategoryId).Select(s => s.Category).SingleOrDefault();
                        if (catEn == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Cannot find Category key " + catRq.CategoryId + " in database");
                        }
                        else
                        {
                            catEn.Key.Lft = catRq.Lft;
                            catEn.Key.Rgt = catRq.Rgt;
                            catEn.Key.NameEn = catRq.NameEn;
                            catEn.Key.NameTh = catRq.NameTh;
                            catEn.Key.UrlKeyEn = catRq.UrlKeyEn;
                            catEn.Key.Visibility = catRq.Visibility;
                            catEn.Key.Status = Constant.STATUS_ACTIVE;
                            catEn.Key.UpdatedBy = this.User.UserRequest().Email;
                            catEn.Key.UpdatedDt = DateTime.Now;
                            catEnList.Remove(catEnList.Where(w => w.Category.Key.CategoryId == catEn.Key.CategoryId).SingleOrDefault());
                        }
                    }
                    else
                    {
                        LocalCategory catEn = new LocalCategory();
                        catEn.Lft = catRq.Lft;
                        catEn.Rgt = catRq.Rgt;
                        catEn.NameEn = catRq.NameEn;
                        catEn.NameTh = catRq.NameTh;
                        catEn.UrlKeyEn = catRq.UrlKeyEn;
                        catEn.ShopId = shopIdRq;
                        catEn.Visibility = catRq.Visibility;
                        catEn.Status = Constant.STATUS_ACTIVE;
                        catEn.CreatedBy = this.User.UserRequest().Email;
                        catEn.CreatedDt = DateTime.Now;
                        catEn.UpdatedBy = this.User.UserRequest().Email;
                        catEn.UpdatedDt = DateTime.Now;
                        db.LocalCategories.Add(catEn);
                    }
                }
                foreach (var cat in catEnList)
                {
                    if (cat.ProductCount != 0)
                    {
                        db.Dispose();
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete category " + cat.Category.Key.NameEn + " with product associated");
                    }
                    db.LocalCategories.Remove(cat.Category.Key);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/LocalCategories/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityCategory(List<CategoryRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = this.User.ShopRequest().ShopId.Value;
                var catList = db.LocalCategories.Where(w=>w.ShopId==shopId).ToList();
                if (catList == null || catList.Count == 0)
                {
                    throw new Exception("No category found in this shop");
                }
                foreach (CategoryRequest catRq in request)
                {

                    var current = catList.Where(w => w.CategoryId == catRq.CategoryId).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find category " + catRq.CategoryId);
                    }
                    current.Visibility = catRq.Visibility.Value;
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/LocalCategories")]
        [HttpDelete]
        public HttpResponseMessage DeteleCategory(List<CategoryRequest> request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                foreach (CategoryRequest rq in request)
                {
                    var cat = db.LocalCategories.Where(w=>w.CategoryId==rq.CategoryId && w.ShopId==shopId).SingleOrDefault();
                    if (cat == null)
                    {
                        throw new Exception("Cannot find category");
                    }
                    int childSize = cat.Rgt.Value - cat.Lft.Value + 1;
                    //delete
                    db.LocalCategories.Where(w => w.Rgt > cat.Rgt && w.ShopId==shopId).ToList()
                        .ForEach(e => { e.Lft = e.Lft > cat.Rgt ? e.Lft - childSize : e.Lft; e.Rgt = e.Rgt - childSize; });
                    db.LocalCategories.RemoveRange(db.LocalCategories.Where(w => w.Lft >= cat.Lft && w.Rgt <= cat.Rgt && w.ShopId==shopId));
                    break;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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

        private bool LocalCategoryExists(int id)
        {
            return db.LocalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}