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
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;

namespace Colsp.Api.Controllers
{
    public class ProductReviewsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ProductReviews")]
        [HttpGet]
        public HttpResponseMessage GetAttributes([FromUri] ProductReviewRequest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var review = (from rev in db.ProductReviews
                              join cus in db.Customers on rev.CustomerId equals cus.CustomerId into cusJoin
                              from cus in cusJoin.DefaultIfEmpty()
                              join stage in db.ProductStages on rev.Pid equals stage.Pid into mastJoin
                             from mast in mastJoin.DefaultIfEmpty()
                             join variant in db.ProductStageVariants on new { rev.Pid, ShopId = shopId } equals new { variant.Pid, variant.ShopId } into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                             select new
                             {
                                 ProductId = mast != null ? mast.ProductId : vari.ProductId,
                                 Sku = vari != null ? vari.Sku : mast.Sku,
                                 Upc = vari != null ? vari.Upc : mast.Upc,
                                 Pid = vari != null ? vari.Pid : mast.Pid,
                                 ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                 ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                 ProductStatus = vari != null ? vari.Status : mast.Status,
                                 rev.ProudctReviewId,
                                 rev.Rating,
                                 ReviewStatus = rev.Status,
                                 cus.CustomerId,
                                 Customer = cus.FirstName + cus.LastName,
                                 rev.UpdatedDt
                             });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, review);
                }
                var total = review.Count();
                var pagedAttribute = review.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
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

        private bool ProductReviewExists(int id)
        {
            return db.ProductReviews.Count(e => e.ProudctReviewId == id) > 0;
        }
    }
}