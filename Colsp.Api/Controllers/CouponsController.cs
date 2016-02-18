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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.InnerException);
            }
        }

        [Route("api/Coupons/{couponId}")]
        [HttpGet]
        public HttpResponseMessage GetCoupon(int couponId)
        {
            try
            {
                var coupon = db.Coupons.Where(w => w.CouponId == couponId && !Constant.STATUS_REMOVE.Equals(w.Status))
                    .Include(i => i.CouponCondition)
                    .Include(i => i.CouponCondition1)
                    .Include(i => i.CouponPidMaps)
                    .Include(i => i.CouponBrandMaps);
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