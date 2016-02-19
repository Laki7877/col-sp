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
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Model.Responses;

namespace Colsp.Api.Controllers
{
    public class CouponsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Coupons")]
        [HttpGet]
        public HttpResponseMessage GetAllCoupon([FromUri] CouponRequest request)
        {
            try
            {
                var coupon = from c in db.Coupons
                             select new
                             {
                                 c.CouponId,
                                 c.CouponCode,
                                 c.CouponName,
                                 Remaining = c.CouponQuantity - c.CouponUsed,
                                 c.StartDate,
                                 c.ExpireDate,
                                 c.Status,
                                 c.ShopId
                             };
                if(this.User.HasPermission("View Promotion"))
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    coupon = coupon.Where(w => w.ShopId == shopId);
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, coupon);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    coupon = coupon.Where(w => w.CouponName.Contains(request.SearchText)
                    || w.CouponCode.Contains(request.SearchText));
                }
                var total = coupon.Count();
                var pagedAttribute = coupon.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Coupons/{couponId}")]
        [HttpGet]
        public HttpResponseMessage GetCoupon(int couponId)
        {
            try
            {
                var coupon = db.Coupons
                    .Where(w => w.CouponId == couponId && !Constant.STATUS_REMOVE.Equals(w.Status))
                    .Select(s=> new {
                        s.CouponName,
                        s.CouponCode,
                        s.Status,
                        s.ExpireDate,
                        s.StartDate,
                        s.ShopId,
                        Action = new { Type = s.Action , s.DiscountAmount, s.MaximumAmount },
                        s.UsagePerCustomer,
                        s.MaximumUser,
                        Conditions = new
                        {
                            Order = s.CouponOrders.Select(se=>new { Type = se.Criteria, Value = se.CriteriaPrice }),
                            FilterBy = new
                            {
                                Type = s.FilterBy,
                                Brands = s.CouponBrandMaps.Select(se=>new { se.Brand.BrandId, se.Brand.BrandNameEn } ),
                                Emails = s.CouponCustomerMaps.Select(se=>se.Email),
                                GlobalCategories = s.CouponGlobalCatMaps.Select(se=>new {se.GlobalCategory.CategoryId, se.GlobalCategory.NameEn }),
                                LocalCategories = s.CouponLocalCatMaps.Select(se=>new { se.LocalCategory.CategoryId, se.LocalCategory.NameEn})
                            },
                            Include = s.CouponPidMaps.Where(w=>w.Filter.Equals("I")).Select(se=>se.Pid),
                            Exclude = s.CouponPidMaps.Where(w=>w.Filter.Equals("E")).Select(se=>se.Pid)
                        }

                    });
                if (this.User.HasPermission("View Promotion"))
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    coupon = coupon.Where(w => w.ShopId == shopId);
                }
                if (coupon == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, coupon.ToList()[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //[Route("api/Coupons")]
        //[HttpPost]
        //public HttpResponseMessage AddCoupon(CouponRequest request)
        //{
        //    Coupon coupon = null;
        //    try
        //    {
        //        if(request == null)
        //        {
        //            throw new Exception("Invalid request");
        //        }
        //        coupon = new Coupon();
        //        var shopId = this.User.ShopRequest().ShopId;
        //        coupon.CouponName = request.CouponName;
        //        coupon.CouponCode = request.CouponCode;
        //        coupon.Status = request.Status;
        //        if (!string.IsNullOrWhiteSpace(request.ExpireDate))
        //        {
        //            coupon.ExpireDate = Convert.ToDateTime(request.ExpireDate);
        //        }
        //        if (!string.IsNullOrWhiteSpace(request.StartDate))
        //        {
        //            coupon.StartDate = Convert.ToDateTime(request.StartDate);
        //        }

        //        coupon.ShopId = shopId;
        //        coupon.Action = request.Action.Type;
        //        coupon.DiscountAmount = request.Action.DiscountAmount;
        //        coupon.MaximumAmount = request.Action.MaximumAmount;
        //        coupon.UsagePerCustomer = request.UsagePerCustomer;
        //        coupon.MaximumUser = request.MaximumUser;

        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CouponExists(int id)
        {
            return db.Coupons.Count(e => e.CouponId == id) > 0;
        }
    }
}