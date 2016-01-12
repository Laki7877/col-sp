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