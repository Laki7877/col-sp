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
using System.Data.Entity.SqlServer;
using System.Data.Entity;
using Colsp.Api.Helpers;

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
                                 c.MaximumUser,
                                 Shop = new { c.Shop.ShopNameEn },
                                 c.CouponType
                             };
                if(User.HasPermission("View Promotion"))
                {
                    var shopId = User.ShopRequest().ShopId;
                    coupon = coupon.Where(w => w.ShopId == shopId && w.CouponType.Equals(Constant.USER_TYPE_SELLER));
                }
                else if(User.HasPermission("Manage Global Coupons"))
                {
                    if (request.IsGlobalCoupon)
                    {
                        coupon = coupon.Where(w => w.CouponType.Equals(Constant.USER_TYPE_ADMIN));
                    }
                    else
                    {
                        coupon = coupon.Where(w => w.CouponType.Equals(Constant.USER_TYPE_SELLER));
                    }
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
                        s.CouponId,
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
                if (User.HasPermission("View Promotion"))
                {
                    var shopId = User.ShopRequest().ShopId;
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
                if(User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    coupon.ShopId = shopId;
                    coupon.CouponType = Constant.USER_TYPE_SELLER;
                }
                else
                {
                    coupon.CouponType = Constant.USER_TYPE_ADMIN;
                }
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                SetupCoupon(coupon, request, email, currentDt, db);
                coupon.CreateBy = email;
                coupon.CreateOn = currentDt;
                coupon.CouponId = db.GetNextCouponId().SingleOrDefault().Value;
                db.Coupons.Add(coupon);
                Util.DeadlockRetry(db.SaveChanges, "Coupon");
                return GetCoupon(coupon.CouponId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Coupons/{couponid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeCoupon(int couponId,CouponRequest request)
        {
            Coupon coupon = null;
            try
            {
                if(User.HasPermission("Edit Promotion"))
                {
                    int shopId = User.ShopRequest().ShopId;
                    coupon = db.Coupons.Where(w => w.CouponId == couponId && w.ShopId == shopId)
                        .Include(i=>i.CouponBrandMaps)
                        .Include(i=>i.CouponCustomerMaps)
                        .Include(i=>i.CouponGlobalCatMaps)
                        .Include(i=>i.CouponLocalCatMaps)
                        .Include(i=>i.CouponPidMaps)
                        .Include(i=>i.CouponShopMaps)
                        .Include(i=>i.CouponOrders).SingleOrDefault();
                }
                else if(User.HasPermission("Manage Global Coupons"))
                {
                    coupon = db.Coupons.Where(w => w.CouponId == couponId)
                        .Include(i => i.CouponBrandMaps)
                        .Include(i => i.CouponCustomerMaps)
                        .Include(i => i.CouponGlobalCatMaps)
                        .Include(i => i.CouponLocalCatMaps)
                        .Include(i => i.CouponPidMaps)
                        .Include(i => i.CouponShopMaps)
                        .Include(i => i.CouponOrders).SingleOrDefault();
                }
                else
                {
                    throw new Exception("You don't have a right permission");
                }
                if (coupon == null)
                {
                    throw new Exception("Cannot find coupon");
                }
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                SetupCoupon(coupon, request, email, currentDt, db);
                Util.DeadlockRetry(db.SaveChanges, "Coupon");
                return GetCoupon(coupon.CouponId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        private void SetupCoupon(Coupon coupon, CouponRequest request, string email, DateTime currentDt, ColspEntities db)
        {
            coupon.CouponName = Validation.ValidateString(request.CouponName, "Coupon Name", true, 300, false);
            coupon.CouponCode = Validation.ValidateString(request.CouponCode, "Coupon Code", true, 50, true); ;
            coupon.Status = Validation.ValidateString(request.Status, "Status", true, 2, true);
            coupon.ExpireDate = Validation.ValidateDateTime(request.ExpireDate, "Expire Date", false);
            coupon.StartDate = Validation.ValidateDateTime(request.StartDate, "Start Date", false);
            coupon.Action = request.Action.Type;
            coupon.DiscountAmount = request.Action.DiscountAmount;
            coupon.MaximumAmount = request.Action.MaximumAmount;
            coupon.UsagePerCustomer = request.UsagePerCustomer;
            coupon.MaximumUser = request.MaximumUser;
            coupon.UpdateBy = email;
            coupon.UpdateOn = currentDt;

            var orderList = coupon.CouponOrders.ToList();
            var brandList = coupon.CouponBrandMaps.ToList();
            var customerList = coupon.CouponCustomerMaps.ToList();
            var globalCatList = coupon.CouponGlobalCatMaps.ToList();
            var localCatList = coupon.CouponLocalCatMaps.ToList();
            var shopList = coupon.CouponShopMaps.ToList();
            var includeList = coupon.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_INCLUDE)).ToList();
            var excludeList = coupon.CouponPidMaps.Where(w => w.Filter.Equals(Constant.COUPON_FILTER_EXCLUDE)).ToList();
            if (request.Conditions != null)
            {
                if (request.Conditions.Order != null && request.Conditions.Order.Count > 0)
                {

                    foreach (OrderRequest o in request.Conditions.Order)
                    {
                        bool addNew = false;
                        if (orderList == null || orderList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            CouponOrder current = orderList.Where(w => w.Criteria == o.Type).SingleOrDefault();
                            if (current != null)
                            {
                                if(current.CriteriaPrice != o.Value)
                                {
                                    current.CriteriaPrice = o.Value;
                                    current.UpdateBy = email;
                                    current.UpdateOn = currentDt;
                                }
                                orderList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            coupon.CouponOrders.Add(new CouponOrder()
                            {
                                Criteria = o.Type,
                                CriteriaPrice = o.Value,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt
                            });
                        }
                    }
                }
                if (request.Conditions.FilterBy != null)
                {
                    coupon.FilterBy = request.Conditions.FilterBy.Type;
                    if ("Brand".Equals(request.Conditions.FilterBy.Type))
                    {
                        if (request.Conditions.FilterBy.Brands != null && request.Conditions.FilterBy.Brands.Count > 0)
                        {

                            foreach (BrandRequest b in request.Conditions.FilterBy.Brands)
                            {
                                bool addNew = false;
                                if (brandList == null || brandList.Count == 0)
                                {
                                    addNew = true;
                                }
                                if (!addNew)
                                {
                                    CouponBrandMap current = brandList.Where(w => w.BrandId == b.BrandId).SingleOrDefault();
                                    if (current != null)
                                    {
                                        brandList.Remove(current);
                                    }
                                    else
                                    {
                                        addNew = true;
                                    }
                                }
                                if (addNew)
                                {
                                    coupon.CouponBrandMaps.Add(new CouponBrandMap()
                                    {
                                        BrandId = b.BrandId,
                                        CreateBy = email,
                                        CreateOn = currentDt,
                                        UpdateBy = email,
                                        UpdateOn = currentDt,
                                    });
                                }
                            }
                        }
                    }
                    else if ("Email".Equals(request.Conditions.FilterBy.Type))
                    {
                        if (request.Conditions.FilterBy.Emails != null && request.Conditions.FilterBy.Emails.Count > 0)
                        {

                            foreach (String e in request.Conditions.FilterBy.Emails)
                            {
                                bool addNew = false;
                                if (customerList == null || customerList.Count == 0)
                                {
                                    addNew = true;
                                }
                                if (!addNew)
                                {
                                    CouponCustomerMap current = customerList.Where(w => w.Email == e).SingleOrDefault();
                                    if (current != null)
                                    {
                                        customerList.Remove(current);
                                    }
                                    else
                                    {
                                        addNew = true;
                                    }
                                }
                                if (addNew)
                                {
                                    coupon.CouponCustomerMaps.Add(new CouponCustomerMap()
                                    {
                                        Email = e,
                                        CreateBy = email,
                                        CreateOn = currentDt,
                                        UpdateBy = email,
                                        UpdateOn = currentDt,
                                    });
                                }
                            }
                        }
                    }
                    else if ("GlobalCategory".Equals(request.Conditions.FilterBy.Type))
                    {

                        if (request.Conditions.FilterBy.GlobalCategories != null && request.Conditions.FilterBy.GlobalCategories.Count > 0)
                        {

                            foreach (CategoryRequest c in request.Conditions.FilterBy.GlobalCategories)
                            {
                                bool addNew = false;
                                if (globalCatList == null || globalCatList.Count == 0)
                                {
                                    addNew = true;
                                }
                                if (!addNew)
                                {
                                    CouponGlobalCatMap current = globalCatList.Where(w => w.CategoryId == c.CategoryId).SingleOrDefault();
                                    if (current != null)
                                    {
                                        globalCatList.Remove(current);
                                    }
                                    else
                                    {
                                        addNew = true;
                                    }
                                }
                                if (addNew)
                                {
                                    coupon.CouponGlobalCatMaps.Add(new CouponGlobalCatMap()
                                    {
                                        Filter = Constant.COUPON_FILTER_INCLUDE,
                                        CategoryId = c.CategoryId,
                                        CreateBy = email,
                                        CreateOn = currentDt,
                                        UpdateBy = email,
                                        UpdateOn = currentDt,
                                    });
                                }
                            }
                        }
                    }
                    else if ("LocalCategory".Equals(request.Conditions.FilterBy.Type))
                    {
                        if (request.Conditions.FilterBy.LocalCategories != null && request.Conditions.FilterBy.LocalCategories.Count > 0)
                        {

                            foreach (CategoryRequest c in request.Conditions.FilterBy.LocalCategories)
                            {
                                bool addNew = false;
                                if (localCatList == null || localCatList.Count == 0)
                                {
                                    addNew = true;
                                }
                                if (!addNew)
                                {
                                    CouponLocalCatMap current = localCatList.Where(w => w.CategoryId == c.CategoryId).SingleOrDefault();
                                    if (current != null)
                                    {
                                        localCatList.Remove(current);
                                    }
                                    else
                                    {
                                        addNew = true;
                                    }
                                }
                                if (addNew)
                                {
                                    coupon.CouponLocalCatMaps.Add(new CouponLocalCatMap()
                                    {
                                        Filter = Constant.COUPON_FILTER_INCLUDE,
                                        CouponId = coupon.CouponId,
                                        CreateBy = email,
                                        CreateOn = currentDt,
                                        UpdateBy = email,
                                        UpdateOn = currentDt,
                                    });
                                }
                            }
                        }
                    }
                    else if ("Shop".Equals(request.Conditions.FilterBy.Type))
                    {

                        if (request.Conditions.FilterBy.Shops != null && request.Conditions.FilterBy.Shops.Count > 0)
                        {

                            foreach (ShopRequest s in request.Conditions.FilterBy.Shops)
                            {
                                bool addNew = false;
                                if (shopList == null || shopList.Count == 0)
                                {
                                    addNew = true;
                                }
                                if (!addNew)
                                {
                                    CouponShopMap current = shopList.Where(w => w.ShopId == s.ShopId).SingleOrDefault();
                                    if (current != null)
                                    {
                                        shopList.Remove(current);
                                    }
                                    else
                                    {
                                        addNew = true;
                                    }
                                }
                                if (addNew)
                                {
                                    coupon.CouponShopMaps.Add(new CouponShopMap()
                                    {
                                        ShopId = s.ShopId,
                                        CreateBy = email,
                                        CreateOn = currentDt,
                                        UpdateBy = email,
                                        UpdateOn = currentDt,
                                    });
                                }
                            }
                        }
                    }
                    if (request.Conditions.Include != null && request.Conditions.Include.Count > 0)
                    {

                        foreach (string pid in request.Conditions.Include)
                        {
                            bool addNew = false;
                            if (includeList == null || includeList.Count == 0)
                            {
                                addNew = true;
                            }
                            if (!addNew)
                            {
                                CouponPidMap current = includeList.Where(w => w.Pid == pid).SingleOrDefault();
                                if (current != null)
                                {
                                    includeList.Remove(current);
                                }
                                else
                                {
                                    addNew = true;
                                }
                            }
                            if (addNew)
                            {
                                coupon.CouponPidMaps.Add(new CouponPidMap()
                                {
                                    Pid = pid,
                                    Filter = Constant.COUPON_FILTER_INCLUDE,
                                    CreateBy = email,
                                    CreateOn = currentDt,
                                    UpdateBy = email,
                                    UpdateOn = currentDt,
                                });
                            }
                        }
                    }
                    if (request.Conditions.Exclude != null && request.Conditions.Exclude.Count > 0)
                    {

                        foreach (string pid in request.Conditions.Exclude)
                        {
                            bool addNew = false;
                            if (excludeList == null || excludeList.Count == 0)
                            {
                                addNew = true;
                            }
                            if (!addNew)
                            {
                                CouponPidMap current = excludeList.Where(w => w.Pid == pid).SingleOrDefault();
                                if (current != null)
                                {
                                    excludeList.Remove(current);
                                }
                                else
                                {
                                    addNew = true;
                                }
                            }
                            if (addNew)
                            {

                                coupon.CouponPidMaps.Add(new CouponPidMap()
                                {
                                    Pid = pid,
                                    Filter = Constant.COUPON_FILTER_EXCLUDE,
                                    CreateBy = email,
                                    CreateOn = currentDt,
                                    UpdateBy = email,
                                    UpdateOn = currentDt,
                                });
                            }
                        }
                    }
                }
                else
                {
                    coupon.FilterBy = string.Empty;
                }
            }

            if (orderList != null && orderList.Count > 0)
            {
                db.CouponOrders.RemoveRange(orderList);
            }
            if (brandList != null && brandList.Count > 0)
            {
                db.CouponBrandMaps.RemoveRange(brandList);
            }
            if (customerList != null && customerList.Count > 0)
            {
                db.CouponCustomerMaps.RemoveRange(customerList);
            }
            if (globalCatList != null && globalCatList.Count > 0)
            {
                db.CouponGlobalCatMaps.RemoveRange(globalCatList);
            }
            if (localCatList != null && localCatList.Count > 0)
            {
                db.CouponLocalCatMaps.RemoveRange(localCatList);
            }
            if (shopList != null && shopList.Count > 0)
            {
                db.CouponShopMaps.RemoveRange(shopList);
            }
            if (includeList != null && includeList.Count > 0)
            {
                db.CouponPidMaps.RemoveRange(includeList);
            }
            if (excludeList != null && excludeList.Count > 0)
            {
                db.CouponPidMaps.RemoveRange(excludeList);
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