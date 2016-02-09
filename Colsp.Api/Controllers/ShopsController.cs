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
using Colsp.Api.Helpers;
using System.Data.SqlClient;

namespace Colsp.Api.Controllers
{
    public class ShopsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Shops")]
        [HttpGet]
        public HttpResponseMessage GetShop([FromUri] ShopRequest request)
        {
            try
            {
                var shopList = db.Shops.Include(i=>i.ShopType).Where(w=>!w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.UpdatedDt
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, shopList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.ShopNameEn))
                {
                    shopList = shopList.Where(a => a.ShopNameEn.Contains(request.ShopNameEn));
                }
                var total = shopList.Count();
                var pagedUsers = shopList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops/{shopId}/ShopTypes")]
        [HttpGet]
        public HttpResponseMessage GetShopType(int shopId)
        {
            try
            {
                var shopType = db.Shops.Where(s => s.ShopId == shopId).Select(s => s.ShopType).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, shopType);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops/{shopId}")]
        [HttpGet]
        public HttpResponseMessage GetShop(int shopId)
        {
            try
            {
                var shop = db.Shops.Include(i=>i.User.UserGroupMaps.Select(ug => ug.UserGroup)).Include(i=>i.ShopType)
                    .Include(i=>i.UserShops.Select(s=>s.User.UserGroupMaps.Select(ug=>ug.UserGroup)))
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s=>new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId,s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        ShopOwner = new
                        {
                            s.User.UserId,
                            s.User.NameEn,
                            s.User.Email,
                            s.User.Phone,
                            s.User.Status,
                            s.User.Position,
                            UserGroup = s.User.UserGroupMaps.Select(ug=>ug.UserGroup.GroupNameEn)
                        },
                        Users = s.UserShops.Select(u=> u.User.Status.Equals(Constant.STATUS_REMOVE) ? null :
                        new
                        {
                            u.User.UserId,
                            u.User.NameEn,
                            u.User.Email,
                            u.User.Status,
                            u.User.Position,
                            UserGroup = u.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        }),
                    }).ToList();
                if(shop == null || shop.Count == 0)
                {
                    throw new Exception("Cannot find shop");
                }
                return Request.CreateResponse(HttpStatusCode.OK, shop[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops")]
        [HttpPost]
        public HttpResponseMessage AddShop(ShopRequest request)
        {
            Shop shop = null;
            User usr = null;
            try
            {
                shop = new Shop();
                shop.Commission = request.Commission;
                shop.ShopNameEn = request.ShopNameEn;
                shop.ShopNameTh = request.ShopNameTh;
                shop.ShopTypeId = request.ShopType.ShopTypeId;
                shop.Status = request.Status;
                shop.CreatedBy = this.User.UserRequest().Email;
                shop.CreatedDt = DateTime.Now;
                shop.UpdatedBy = this.User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                usr = new User();
                usr.Email = Validation.ValidateString(request.ShopOwner.Email, "Email", true, 100, false);
                usr.Password = request.ShopOwner.Password;
                usr.NameEn = request.ShopOwner.NameEn;
                usr.NameTh = request.ShopOwner.NameTh;
                usr.Division = request.ShopOwner.Division;
                usr.Phone = request.ShopOwner.Phone;
                usr.Position = request.ShopOwner.Position;
                usr.EmployeeId = request.ShopOwner.EmployeeId;
                usr.Status = Constant.STATUS_ACTIVE;
                usr.Type = Constant.USER_TYPE_SELLER;
                usr.CreatedBy = this.User.UserRequest().Email;
                usr.CreatedDt = DateTime.Now;
                usr.UpdatedBy = this.User.UserRequest().Email;
                usr.UpdatedDt = DateTime.Now;
                db.Users.Add(usr);
                db.SaveChanges();
               
                UserGroupMap map = new UserGroupMap();
                map.UserId = usr.UserId;
                map.GroupId = Constant.SHOP_OWNER_GROUP_ID;
                map.CreatedBy = this.User.UserRequest().Email;
                map.CreatedDt = DateTime.Now;
                map.UpdatedBy = this.User.UserRequest().Email;
                map.UpdatedDt = DateTime.Now;
                db.UserGroupMaps.Add(map);
                shop.ShopOwner = usr.UserId;
                db.Shops.Add(shop);
                db.SaveChanges();

                UserShop userShop = new UserShop();
                userShop.ShopId = shop.ShopId;
                userShop.UserId = usr.UserId;
                userShop.CreatedBy = this.User.UserRequest().Email;
                userShop.CreatedDt = DateTime.Now;
                userShop.UpdatedBy = this.User.UserRequest().Email;
                userShop.UpdatedDt = DateTime.Now;
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                if (usr != null && usr.UserId != 0)
                {
                    db.Users.Remove(usr);
                }
                if (shop != null && shop.ShopId != 0)
                {
                    db.Shops.Remove(shop);
                }
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops/{shopId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeShop([FromUri]int shopId, ShopRequest request)
        {
            User usr = null;
            Shop shop = null;
            try
            {
                var shopList = db.Shops
                    .Include(i=>i.User.UserGroupMaps)
                    .Include(i=>i.UserShops)
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE)).ToList();
                if (shopList == null || shopList.Count == 0)
                {
                    throw new Exception("Shop not found");
                }
                shop = shopList[0];
                if(shop.User != null)
                {
                    if (shop.User.Email != null && shop.User.Email.Equals(request.ShopOwner.Email))
                    {
                        if (!string.IsNullOrEmpty(request.ShopOwner.Password))
                        {
                            shop.User.Password = request.ShopOwner.Password;
                        }
                        shop.User.NameEn = request.ShopOwner.NameEn;
                        shop.User.NameTh = request.ShopOwner.NameTh;
                        shop.User.Division = request.ShopOwner.Division;
                        shop.User.Phone = request.ShopOwner.Phone;
                        shop.User.Position = request.ShopOwner.Position;
                        shop.User.EmployeeId = request.ShopOwner.EmployeeId;
                        shop.User.Status = Constant.STATUS_ACTIVE;
                        shop.User.Type = Constant.USER_TYPE_SELLER;
                    }
                    else
                    {
                        shop.User.Status = Constant.STATUS_REMOVE;
                        db.UserGroupMaps.RemoveRange(shop.User.UserGroupMaps);
                        db.UserShops.RemoveRange(shop.UserShops);
                        usr = new User();
                        usr.Email = Validation.ValidateString(request.ShopOwner.Email, "Email", true, 100, false);
                        usr.Password = request.ShopOwner.Password;
                        usr.NameEn = request.ShopOwner.NameEn;
                        usr.NameTh = request.ShopOwner.NameTh;
                        usr.Division = request.ShopOwner.Division;
                        usr.Phone = request.ShopOwner.Phone;
                        usr.Position = request.ShopOwner.Position;
                        usr.EmployeeId = request.ShopOwner.EmployeeId;
                        usr.Status = Constant.STATUS_ACTIVE;
                        usr.Type = Constant.USER_TYPE_SELLER;
                        usr.CreatedBy = this.User.UserRequest().Email;
                        usr.CreatedDt = DateTime.Now;
                        usr.UpdatedBy = this.User.UserRequest().Email;
                        usr.UpdatedDt = DateTime.Now;
                        db.Users.Add(usr);
                        db.SaveChanges();
                        UserGroupMap map = new UserGroupMap();
                        map.UserId = usr.UserId;
                        map.GroupId = Constant.SHOP_OWNER_GROUP_ID;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        db.UserGroupMaps.Add(map);
                        shop.ShopOwner = usr.UserId;

                        UserShop userShop = new UserShop();
                        userShop.ShopId = shop.ShopId;
                        userShop.UserId = usr.UserId;
                        userShop.CreatedBy = this.User.UserRequest().Email;
                        userShop.CreatedDt = DateTime.Now;
                        userShop.UpdatedBy = this.User.UserRequest().Email;
                        userShop.UpdatedDt = DateTime.Now;
                    }
                }
                shop.Commission = request.Commission;
                shop.ShopNameEn = request.ShopNameEn;
                shop.ShopNameTh = request.ShopNameTh;
                shop.ShopTypeId = request.ShopType.ShopTypeId;
                shop.Status = request.Status;
                shop.UpdatedBy = this.User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                db.SaveChanges();
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                if (usr != null && usr.UserId != 0)
                {
                    db.Users.Remove(usr);
                }
                if (shop != null && shop.ShopId != 0)
                {
                    db.Shops.Remove(shop);
                }
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/Shops/{shopId}/LocalCategories")]
        [HttpGet]
        public HttpResponseMessage GetCategoryFromShop(int shopId)
        {
            try
            {
                var localCat = (from cat in db.LocalCategories
                                 where cat.ShopId == shopId
                                 select new
                                 {
                                     cat.CategoryId,
                                     cat.NameEn,
                                     cat.NameTh,
                                     cat.Lft,
                                     cat.Rgt,
                                     cat.UrlKeyEn,
                                     cat.UrlKeyTh,
                                     cat.Visibility,
                                     cat.Status,
                                     cat.UpdatedDt,
                                     cat.CreatedDt,
                                     ProductCount = cat.ProductStages.Count,
                                 }).ToList();
                if (localCat != null && localCat.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, localCat);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }

            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Shops/{shopId}/LocalCategories")]
        [HttpPut]
        public HttpResponseMessage SaveChangeLocalCategory([FromUri]int shopId, List<CategoryRequest> request)
        {
            try
            {
                int shopIdRq = this.User.ShopRequest().ShopId.Value;
                if (shopIdRq == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Shop is invalid. Cannot find shop in session");
                }
                var catEnList = (from cat in db.LocalCategories
                                 join proStg in db.ProductStages on cat.CategoryId equals proStg.LocalCatId into j
                                 from j2 in j.DefaultIfEmpty()
                                 where cat.ShopId == shopIdRq
                                 group j2 by cat into g
                                 select new
                                 {
                                     Category = g,
                                     ProductCount = g.Key.ProductStages.Count
                                 }).ToList();
                foreach (CategoryRequest catRq in request)
                {
                    if (catRq.Lft == null || catRq.Rgt == null || catRq.Lft >= catRq.Rgt)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node is not properly formated");
                    }
                    var validate = request.Where(w => w.Lft == catRq.Lft || w.Rgt == catRq.Rgt).ToList();
                    if (validate != null && validate.Count > 1)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Node child has duplicated left or right key");
                    }
                    if (catRq.CategoryId != 0)
                    {
                        var catEn = catEnList.Where(w => w.Category.Key.CategoryId == catRq.CategoryId).Select(s => s.Category).SingleOrDefault();
                        if (catEn == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Category " + catRq.NameEn + " is invalid. Cannot find Category key " + catRq.CategoryId + " in database");
                        }
                        else
                        {
                            catEn.Key.Lft = catRq.Lft;
                            catEn.Key.Rgt = catRq.Rgt;
                            catEn.Key.NameEn = catRq.NameEn;
                            catEn.Key.NameTh = catRq.NameTh;
                            catEn.Key.UrlKeyEn = catRq.UrlKeyEn;
                            catEn.Key.Visibility = catRq.Visibility;
                            catEn.Key.Status = Constant.STATUS_ACTIVE;
                            catEn.Key.UpdatedBy = this.User.UserRequest().Email;
                            catEn.Key.UpdatedDt = DateTime.Now;
                            catEnList.Remove(catEnList.Where(w => w.Category.Key.CategoryId == catEn.Key.CategoryId).SingleOrDefault());
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
                        catEn.ShopId = shopId;
                        catEn.Visibility = catRq.Visibility;
                        catEn.Status = Constant.STATUS_ACTIVE;
                        catEn.CreatedBy = this.User.UserRequest().Email;
                        catEn.CreatedDt = DateTime.Now;
                        catEn.UpdatedBy = this.User.UserRequest().Email;
                        catEn.UpdatedDt = DateTime.Now;
                        db.LocalCategories.Add(catEn);
                    }
                }
                foreach (var cat in catEnList)
                {
                    if (cat.ProductCount != 0)
                    {
                        db.Dispose();
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot delete category " + cat.Category.Key.NameEn + " with product associated");
                    }
                    db.LocalCategories.Remove(cat.Category.Key);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        //[Route("api/Shops/{sellerId}/ProductStages")]
        //[HttpGet]
        //public HttpResponseMessage GetProductStageFromShop([FromUri] ProductRequest request)
        //{
        //    try
        //    {
        //        request.DefaultOnNull();
        //        IQueryable<ProductStage> products = null;

        //        // List all product
        //        products = db.ProductStages.Where(p => true);
        //        if (request.SearchText != null)
        //        {
        //            products = products.Where(p => p.Sku.Contains(request.SearchText)
        //            || p.ProductNameEn.Contains(request.SearchText)
        //            || p.ProductNameTh.Contains(request.SearchText));
        //        }
        //        if (request.SellerId != null)
        //        {
        //            products = products.Where(p => p.SellerId==request.SellerId);
        //        }

        //        var total = products.Count();
        //        var pagedProducts = products.GroupJoin(db.ProductStageImages,
        //                                        p => p.Pid,
        //                                        m => m.Pid,
        //                                        (p, m) => new
        //                                        {
        //                                            p.Sku,
        //                                            p.ProductId,
        //                                            p.ProductNameEn,
        //                                            p.ProductNameTh,
        //                                            p.OriginalPrice,
        //                                            p.SalePrice,
        //                                            p.Status,
        //                                            p.ImageFlag,
        //                                            p.InfoFlag,
        //                                            Modified = p.UpdatedDt,
        //                                            ImageUrl = m.FirstOrDefault().ImageUrlEn
        //                                        }
        //                                    )
        //                                    .Paginate(request);
        //        var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
        //        return Request.CreateResponse(HttpStatusCode.OK, response);
        //    }
        //    catch
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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

        private bool ShopExists(int id)
        {
            return db.Shops.Count(e => e.ShopId == id) > 0;
        }
    }
}