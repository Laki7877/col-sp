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
                var review = (from stage in db.ProductStages
                             let rev = db.ProductReviews.Where(w => w.Pid.Equals(stage.Pid)).FirstOrDefault()
                             join variant in db.ProductStageVariants on stage.ProductId equals variant.ProductId into varJoin
                             from varJ in varJoin.DefaultIfEmpty()
                             let varInv = db.ProductReviews.Where(w => w.Pid.Equals(varJ.Pid)).FirstOrDefault()
                             where stage.ShopId == shopId
                             select new
                             {
                                 stage.ProductId,
                                 Sku = varJ != null ? varJ.Sku : stage.Sku,
                                 Upc = varJ != null ? varJ.Upc : stage.Upc,
                                 Pid = varJ != null ? varJ.Pid : stage.Pid,
                                 ProductNameEn = varJ != null ? varJ.ProductNameEn : stage.ProductNameEn,
                                 ProductNameTh = varJ != null ? varJ.ProductNameTh : stage.ProductNameTh,
                                 Status = varJ != null ? varJ.Status : stage.Status,
                                 Review = rev != null ? rev : varInv
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