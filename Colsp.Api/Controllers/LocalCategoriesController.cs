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

namespace Colsp.Api.Controllers
{
    public class LocalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        

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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/LocalCategories")]
        [HttpPut]
        public HttpResponseMessage SaveChange(List<CategoryRequest> request)
        {
            try
            {
                foreach(CategoryRequest catRq in request)
                {
                    if(catRq.ShopId == null || this.User.ShopIds().Where(w => w == catRq.ShopId).SingleOrDefault() == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Shop is invalid");
                    }
                    if(catRq.Lft == null || catRq.Rgt == null || catRq.Lft >= catRq.Rgt)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Category is invalid");
                    }
                    var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
                    if(validate != null && validate.Count > 1)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Category is invalid");
                    }
                    if(catRq.CategoryId != 0)
                    {
                        var catEn = db.LocalCategories.Where(w => w.CategoryId == catRq.CategoryId && w.ShopId == catRq.ShopId).SingleOrDefault();
                        if(catEn == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Category is invalid");
                        }
                        else
                        {
                            catEn.Lft = catRq.Lft;
                            catEn.Rgt = catRq.Rgt;
                            catEn.NameEn = catRq.NameEn;
                            catEn.NameTh = catRq.NameTh;
                            catEn.UrlKeyEn = catRq.UrlKeyEn;
                            catEn.Status = catRq.Status;
                            catEn.UpdatedBy = this.User.Email();
                            catEn.UpdatedDt = DateTime.Now;
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
                        catEn.ShopId = catRq.ShopId;
                        catEn.Status = catRq.Status;
                        catEn.CreatedBy = this.User.Email();
                        catEn.CreatedDt = DateTime.Now;
                        db.LocalCategories.Add(catEn);
                    }
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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