using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.Entity.SqlServer;

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
                                 c.ShopId,
                                 c.CouponType
                             };
                if(this.User.HasPermission("View Promotion"))
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    coupon = coupon.Where(w => w.ShopId == shopId && w.CouponType.Equals(Constant.USER_TYPE_SELLER));
                }
                else if(this.User.HasPermission("Manage Global Coupons"))
                {
                    coupon = coupon.Where(w => w.CouponType.Equals(Constant.USER_TYPE_ADMIN));
                }
                else
                {
                    throw new Exception("You don't have a right permission");
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, coupon);
                }
                request.DefaultOnNull();
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
                var couponList = db.Coupons
                    .Where(w => w.CouponId == couponId && !Constant.STATUS_REMOVE.Equals(w.Status))
                    .Select(s=> new {
                        s.CouponName,
                        s.CouponCode,
                        s.Status,
                        StartDate = SqlFunctions.DateName("month", s.StartDate) + " " +  SqlFunctions.DateName("day", s.StartDate) + ", " + SqlFunctions.DateName("year", s.StartDate) + " " + SqlFunctions.DateName("hour", s.StartDate) + ":" + SqlFunctions.DateName("minute", s.StartDate),
                        ExpireDate = SqlFunctions.DateName("month", s.ExpireDate) + " " + SqlFunctions.DateName("day", s.ExpireDate) + ", " + SqlFunctions.DateName("year", s.ExpireDate) + " " + SqlFunctions.DateName("hour", s.ExpireDate) + ":" + SqlFunctions.DateName("minute", s.ExpireDate),
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
                                LocalCategories = s.CouponLocalCatMaps.Select(se=>new { se.LocalCategory.CategoryId, se.LocalCategory.NameEn}),
                                Shops = s.CouponShopMaps.Select(se=>new { se.Shop.ShopId,se.Shop.ShopNameEn })
                            },
                            Include = s.CouponPidMaps.Where(w=>w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE)).Select(se=>se.Pid),
                            Exclude = s.CouponPidMaps.Where(w=>w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE)).Select(se=>se.Pid)
                        }

                    });
                if (this.User.HasPermission("View Promotion"))
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    couponList = couponList.Where(w => w.ShopId == shopId);
                }
                var c = couponList.ToList();
                if(c == null || c.Count == 0)
                {
                    throw new Exception("Cannot find coupon");
                }
                return Request.CreateResponse(HttpStatusCode.OK, c[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Coupons")]
        [HttpPost]
        public HttpResponseMessage AddCoupon(CouponRequest request)
        {
            Coupon coupon = null;
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                coupon = new Coupon();
                if(this.User.HasPermission("Edit Promotion"))
                {
                    var shopId = this.User.ShopRequest().ShopId;
                    coupon.ShopId = shopId;
                    coupon.CouponType = Constant.USER_TYPE_SELLER;
                }
                else if (this.User.HasPermission("Manage Global Coupons"))
                {
                    coupon.CouponType = Constant.USER_TYPE_ADMIN;
                }
                else
                {
                    throw new Exception("You don't have a right permission");
                }
                coupon.CouponName = request.CouponName;
                coupon.CouponCode = request.CouponCode;
                coupon.Status = request.Status;
                if (!string.IsNullOrWhiteSpace(request.ExpireDate))
                {
                    coupon.ExpireDate = Convert.ToDateTime(request.ExpireDate);
                }
                if (!string.IsNullOrWhiteSpace(request.StartDate))
                {
                    coupon.StartDate = Convert.ToDateTime(request.StartDate);
                }
                coupon.Action = request.Action.Type;
                coupon.DiscountAmount = request.Action.DiscountAmount;
                coupon.MaximumAmount = request.Action.MaximumAmount;
                coupon.UsagePerCustomer = request.UsagePerCustomer;
                coupon.MaximumUser = request.MaximumUser;
                coupon.CreatedBy = this.User.UserRequest().Email;
                coupon.CreatedDt = DateTime.Now;
                coupon.UpdatedBy = this.User.UserRequest().Email;
                coupon.UpdatedDt = DateTime.Now;
                coupon = db.Coupons.Add(coupon);
                db.SaveChanges();

                if(request.Conditions != null)
                {
                    if(request.Conditions.Order != null && request.Conditions.Order.Count > 0)
                    {
                        foreach(OrderRequest o in request.Conditions.Order)
                        {
                            CouponOrder co = new CouponOrder();
                            co.CouponId = coupon.CouponId;
                            co.Criteria = o.Type;
                            co.CriteriaPrice = o.Value;
                            co.CreatedBy = this.User.UserRequest().Email;
                            co.CreatedDt = DateTime.Now;
                            co.UpdatedBy = this.User.UserRequest().Email;
                            co.UpdatedDt = DateTime.Now;
                            db.CouponOrders.Add(co);
                        }
                    }
                    if(request.Conditions.FilterBy != null)
                    {
                        if ("Brand".Equals(request.Conditions.FilterBy.Type))
                        {
                            if(request.Conditions.FilterBy.Brands != null && request.Conditions.FilterBy.Brands.Count > 0)
                            {
                                foreach (BrandRequest b in request.Conditions.FilterBy.Brands)
                                {
                                    CouponBrandMap map = new CouponBrandMap();
                                    map.CouponId = coupon.CouponId;
                                    map.BrandId = b.BrandId.Value;
                                    map.CreatedBy = this.User.UserRequest().Email;
                                    map.CreatedDt = DateTime.Now;
                                    map.UpdatedBy = this.User.UserRequest().Email;
                                    map.UpdatedDt = DateTime.Now;
                                    db.CouponBrandMaps.Add(map);
                                }
                            }
                        }
                        else if ("Email".Equals(request.Conditions.FilterBy.Type))
                        {
                            if (request.Conditions.FilterBy.Emails != null && request.Conditions.FilterBy.Emails.Count > 0)
                            {
                                foreach (String e in request.Conditions.FilterBy.Emails)
                                {
                                    CouponCustomerMap map = new CouponCustomerMap();
                                    map.CouponId = coupon.CouponId;
                                    map.Email = e;
                                    map.CreatedBy = this.User.UserRequest().Email;
                                    map.CreatedDt = DateTime.Now;
                                    map.UpdatedBy = this.User.UserRequest().Email;
                                    map.UpdatedDt = DateTime.Now;
                                    db.CouponCustomerMaps.Add(map);
                                }
                            }
                        }
                        else if ("GlobalCategory".Equals(request.Conditions.FilterBy.Type))
                        {
                            if (request.Conditions.FilterBy.GlobalCategories != null && request.Conditions.FilterBy.GlobalCategories.Count > 0)
                            {
                                foreach (CategoryRequest c in request.Conditions.FilterBy.GlobalCategories)
                                {
                                    CouponGlobalCatMap map = new CouponGlobalCatMap();
                                    map.CouponId = coupon.CouponId;
                                    map.CategoryId = c.CategoryId.Value;
                                    map.CreatedBy = this.User.UserRequest().Email;
                                    map.CreatedDt = DateTime.Now;
                                    map.UpdatedBy = this.User.UserRequest().Email;
                                    map.UpdatedDt = DateTime.Now;
                                    db.CouponGlobalCatMaps.Add(map);
                                }
                            }
                        }
                        else if ("LocalCategory".Equals(request.Conditions.FilterBy.Type))
                        {
                            if (request.Conditions.FilterBy.LocalCategories != null && request.Conditions.FilterBy.LocalCategories.Count > 0)
                            {
                                foreach (CategoryRequest c in request.Conditions.FilterBy.LocalCategories)
                                {
                                    CouponLocalCatMap map = new CouponLocalCatMap();
                                    map.CouponId = coupon.CouponId;
                                    map.CategoryId = c.CategoryId.Value;
                                    map.CreatedBy = this.User.UserRequest().Email;
                                    map.CreatedDt = DateTime.Now;
                                    map.UpdatedBy = this.User.UserRequest().Email;
                                    map.UpdatedDt = DateTime.Now;
                                    db.CouponLocalCatMaps.Add(map);
                                }
                            }
                        }
                        else if ("Shop".Equals(request.Conditions.FilterBy.Type))
                        {
                            if (request.Conditions.FilterBy.Shops != null && request.Conditions.FilterBy.Shops.Count > 0)
                            {
                                foreach (ShopRequest s in request.Conditions.FilterBy.Shops)
                                {
                                    CouponShopMap map = new CouponShopMap();
                                    map.CouponId = coupon.CouponId;
                                    map.ShopId = s.ShopId.Value;
                                    map.CreatedBy = this.User.UserRequest().Email;
                                    map.CreatedDt = DateTime.Now;
                                    map.UpdatedBy = this.User.UserRequest().Email;
                                    map.UpdatedDt = DateTime.Now;
                                    db.CouponShopMaps.Add(map);
                                }
                            }
                        }
                        if (request.Conditions.FilterBy.Include != null && request.Conditions.FilterBy.Include.Count  > 0)
                        {
                            foreach(string pid in request.Conditions.FilterBy.Include)
                            {
                                CouponPidMap map = new CouponPidMap();
                                map.CouponId = coupon.CouponId;
                                map.Pid = pid;
                                map.Filter = Constant.COUPON_FILTER_INCLUDE;
                                map.CreatedBy = this.User.UserRequest().Email;
                                map.CreatedDt = DateTime.Now;
                                map.UpdatedBy = this.User.UserRequest().Email;
                                map.UpdatedDt = DateTime.Now;
                                db.CouponPidMaps.Add(map);
                            }
                        }
                        if (request.Conditions.FilterBy.Exclude != null && request.Conditions.FilterBy.Exclude.Count > 0)
                        {
                            foreach (string pid in request.Conditions.FilterBy.Exclude)
                            {
                                CouponPidMap map = new CouponPidMap();
                                map.CouponId = coupon.CouponId;
                                map.Pid = pid;
                                map.Filter = Constant.COUPON_FILTER_EXCLUDE;
                                map.CreatedBy = this.User.UserRequest().Email;
                                map.CreatedDt = DateTime.Now;
                                map.UpdatedBy = this.User.UserRequest().Email;
                                map.UpdatedDt = DateTime.Now;
                                db.CouponPidMaps.Add(map);
                            }
                        }
                    }
                    
                    db.SaveChanges();
                }
                return GetCoupon(coupon.CouponId);
            }
            catch (DbUpdateException e)
            {
                if (coupon != null && coupon.CouponId != 0)
                {
                    db.Coupons.Remove(coupon);
                    db.SaveChanges();
                }
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "Coupon code has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                if (coupon != null && coupon.CouponId != 0)
                {
                    db.Coupons.Remove(coupon);
                    db.SaveChanges();
                }
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