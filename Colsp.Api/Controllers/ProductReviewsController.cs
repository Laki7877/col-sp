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
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class ProductReviewsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ProductReviews")]
        [HttpGet]
        public HttpResponseMessage GetReviews([FromUri] ProductReviewRequest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId.Value;
                var review = (from rev in db.ProductReviews
                              join cus in db.Customers on rev.CustomerId equals cus.CustomerId into cusJoin
                              from cus in cusJoin.DefaultIfEmpty()
                              join stage in db.ProductStages on new { rev.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId } into mastJoin
                              from mast in mastJoin.DefaultIfEmpty()
                              join variant in db.ProductStageVariants on new { rev.Pid, ShopId = shopId } equals new { variant.Pid, variant.ShopId } into varJoin
                              from vari in varJoin.DefaultIfEmpty()
                              where mast != null || vari != null
                              select new
                              {
                                  ProductId = mast != null ? mast.ProductId : vari != null ? vari.ProductId : 0,
                                  Sku = vari != null ? vari.Sku : mast.Sku,
                                  Upc = vari != null ? vari.Upc : mast.Upc,
                                  Pid = vari != null ? vari.Pid : mast.Pid,
                                  ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                  ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                  BrandId = vari != null && vari.ProductStage != null && vari.ProductStage.Brand != null ? vari.ProductStage.Brand.BrandId : mast != null && mast.Brand != null ? mast.Brand.BrandId : 0,
                                  BrandNameEn = vari != null ? vari.ProductStage.Brand.BrandNameEn : mast.Brand.BrandNameEn,
                                  rev.ProductReviewId,
                                  rev.Comment,
                                  rev.Rating,
                                  rev.Status,
                                  cus.CustomerId,
                                  Customer = cus != null ? cus.FirstName + " " + cus.LastName : null,
                                  UpdatedDt = rev.CreatedDt
                              });

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, review);
                }
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    review = review.Where(w => w.Comment.Contains(request.SearchText)
                    || w.Pid.Contains(request.SearchText)
                    || w.Customer.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        review = review.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        review = review.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                }
                var total = review.Count();
                var pagedAttribute = review.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.InnerException);
            }
        }

        [Route("api/ProductReviews/Approve")]
        [HttpPut]
        public HttpResponseMessage ProductReviewApprove(List<ProductReviewRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId.Value;
                var review = (from rev in db.ProductReviews
                              join cus in db.Customers on rev.CustomerId equals cus.CustomerId into cusJoin
                              from cus in cusJoin.DefaultIfEmpty()
                              join stage in db.ProductStages on new { rev.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId } into mastJoin
                              from mast in mastJoin.DefaultIfEmpty()
                              join variant in db.ProductStageVariants on new { rev.Pid, ShopId = shopId } equals new { variant.Pid, variant.ShopId } into varJoin
                              from vari in varJoin.DefaultIfEmpty()
                              select rev
                              ).ToList();
                if (review == null || review.Count == 0)
                {
                    throw new Exception("No review found");
                }
                foreach (ProductReviewRequest revRq in request)
                {

                    var current = review.Where(w => w.ProductReviewId.Equals(revRq.ProductReviewId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find review " + revRq.ProductReviewId + " in shop " + shopId);
                    }
                    current.Status = revRq.Status;
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
            return db.ProductReviews.Count(e => e.ProductReviewId == id) > 0;
        }
    }
}