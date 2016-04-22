using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Api.Helpers;

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
                //var shopId = User.ShopRequest().ShopId;
                var review = (from rev in db.ProductReviews
                              join cus in db.Customers on rev.CustomerId equals cus.CustomerId into cusJoin
                              from cus in cusJoin.DefaultIfEmpty()
                              join stage in db.Products on rev.Pid equals stage.Pid into mastJoin
                              from stage in mastJoin.DefaultIfEmpty()
                              join brand in db.Brands on stage.BrandId equals brand.BrandId into brandJoin
                              from brand in brandJoin.DefaultIfEmpty()
                              select new
                              {
                                  //ProductId = stage.ProductId,
                                  Sku = stage.Sku,
                                  Upc = stage.Upc,
                                  Pid = stage.Pid,
                                  ProductNameEn = stage.ProductNameEn,
                                  ProductNameTh = stage.ProductNameTh,
                                  Brand = brand == null ? null : new { brand.BrandId, brand.BrandNameEn },
                                  rev.ProductReviewId,
                                  rev.Comment,
                                  rev.ProductContent,
                                  rev.ProductValidity,
                                  rev.DeliverySpeed,
                                  rev.Packaging,
                                  rev.Status,
                                  Customer = cus == null ? null : new
                                  {
                                      Name = cus.FirstName + " " + cus.LastName,
                                      CustomerId = cus.CustomerId,
                                  },
                                  Shop =  new
                                  {
                                      rev.ShopId,
                                      rev.Shop.ShopNameEn
                                  },
                                  UpdatedDt = rev.CreatedOn
                              });
                if(User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    review = review.Where(w => w.Shop.ShopId == shopId);
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, review);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    review = review.Where(w => w.Comment.Contains(request.SearchText)
                    || w.Pid.Contains(request.SearchText)
                    || w.Customer.Name.Contains(request.SearchText));
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        //[Route("api/ProductReviews/Approve")]
        //[HttpPut]
        //public HttpResponseMessage ProductReviewApprove(List<ProductReviewRequest> request)
        //{
        //    try
        //    {
        //        if (request == null || request.Count == 0)
        //        {
        //            throw new Exception("Invalid request");
        //        }
        //        var shopId = User.ShopRequest().ShopId;
        //        var review = (from rev in db.ProductReviews
        //                      join cus in db.Customers on rev.CustomerId equals cus.CustomerId into cusJoin
        //                      from cus in cusJoin.DefaultIfEmpty()
        //                      join stage in db.ProductStages on new { rev.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId } into mastJoin
        //                      from mast in mastJoin.DefaultIfEmpty()
        //                      select rev
        //                      ).ToList();
        //        if (review == null || review.Count == 0)
        //        {
        //            throw new Exception("No review found");
        //        }
        //        foreach (ProductReviewRequest revRq in request)
        //        {

        //            var current = review.Where(w => w.ProductReviewId.Equals(revRq.ProductReviewId)).SingleOrDefault();
        //            if (current == null)
        //            {
        //                throw new Exception("Cannot find review " + revRq.ProductReviewId + " in shop " + shopId);
        //            }
        //            current.Status = revRq.Status;
        //            current.UpdatedBy = User.UserRequest().Email;
        //            current.UpdatedOn = DateTime.Now;
        //        }
        //        Util.DeadlockRetry(db.SaveChanges, "ProductReview");
        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
        //    }
        //}


        [Route("api/ProductReviews/{ProductReviewId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeRating([FromUri] int ProductReviewId, ProductReviewRequest request)
        {
            try
            {
                var review = db.ProductReviews.Where(w => w.ProductReviewId == ProductReviewId).SingleOrDefault();
                if(review == null)
                {
                    throw new Exception("Cannot find review");
                }
                review.ProductContent = request.ProductContent;
                review.ProductValidity = request.ProductValidity;
                review.DeliverySpeed = request.DeliverySpeed;
                review.Packaging = request.Packaging;
                Util.DeadlockRetry(db.SaveChanges, "ProductReview");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/ProductReviews/Rating")]
        [HttpGet]
        public HttpResponseMessage GetAverageRating()
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var rating = db.ProductReviews.Where(w=>w.ShopId==shopId).Select(s=>new
                {
                    s.ProductContent,
                    s.ProductValidity,
                    s.DeliverySpeed,
                    s.Packaging,
                }).ToList();
                if(rating == null || rating.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Constant.NOT_AVAILABLE);
                }

                var average = new
                {
                    ProductContent = rating.Select(s=>s.ProductContent).Average(),
                    ProductValidity = rating.Select(s => s.ProductValidity).Average(),
                    DeliverySpeed = rating.Select(s => s.DeliverySpeed).Average(),
                    Packaging = rating.Select(s => s.Packaging).Average(),
                };

                return Request.CreateResponse(HttpStatusCode.OK, average);
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