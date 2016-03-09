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
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Configuration;
using System.Data.Entity.SqlServer;
using Colsp.Api.Services;

namespace Colsp.Api.Controllers
{
    public class ShopsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        //private readonly string root = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[AppSettingKey.IMAGE_ROOT_PATH]);

        [Route("api/ShopImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                FileUploadRespond fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.SHOP_FOLDER, 1500, 1500, 2000, 2000, 5, true);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

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
                        s.ShopGroup,
                        s.UpdatedDt,
                        s.BankAccountName,
                        s.BankAccountNumber
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, shopList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    shopList = shopList.Where(a => a.ShopNameEn.Contains(request.SearchText) 
                    || SqlFunctions.StringConvert((double)a.ShopId).Contains(request.SearchText)
                    || a.ShopType.ShopTypeNameEn.Contains(request.SearchText));
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
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        s.BankName,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        Commissions = s.ShopCommissions.Select(sc=>new { sc.CategoryId,sc.Commission}),
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

        [Route("api/Shop/Profile")]
        [HttpGet]
        public HttpResponseMessage GetShopProfile()
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var shop = db.Shops
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.ShopAddress,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.Facebook,
                        s.Youtube,
                        s.Instagram,
                        s.Pinterest,
                        s.Twitter,
                        s.StockAlert,
                        s.FloatMessageEn,
                        s.FloatMessageTh,
                        GiftWrap = s.GiftWrap == true ? "Available" : "NotAvailable",
                        TaxInvoice = s.TaxInvoice == true ? "Available" : "NotAvailable",
                        Logo = new ImageRequest { url=s.ShopImageUrl },
                        s.Status
                    }).ToList();
                if (shop == null || shop.Count == 0)
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

        [Route("api/Shop/Profile")]
        [HttpPut]
        public HttpResponseMessage SaveShopProfile(ShopRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var shop = db.Shops.Find(shopId);
                if (shop == null || shop.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("Cannot find shop");
                }
                shop.ShopNameEn = request.ShopNameEn;
                if ("Available".Equals(request.GiftWrap))
                {
                    shop.GiftWrap = true;
                }
                if ("Available".Equals(request.TaxInvoice))
                {
                    shop.TaxInvoice = true;
                }
                shop.ShopDescriptionEn = request.ShopDescriptionEn;
                shop.ShopDescriptionTh = request.ShopDescriptionTh;
                shop.FloatMessageEn = request.FloatMessageEn;
                shop.FloatMessageTh = request.FloatMessageTh;
                shop.ShopAddress = request.ShopAddress;
                //shop.BankAccountName = request.BankAccountName;
                //shop.BankAccountNumber = request.BankAccountNumber;
                shop.Facebook = request.Facebook;
                shop.Youtube = request.Youtube;
                shop.Instagram = request.Instagram;
                shop.Pinterest = request.Pinterest;
                shop.Twitter = request.Twitter;
                shop.StockAlert = Validation.ValidationInteger(request.StockAlert, "Stock Alert", true, Int32.MaxValue, 0).Value;
                shop.Status = request.Status;
                shop.UpdatedBy = this.User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                if(request.Logo != null)
                {
                    shop.ShopImageUrl = request.Logo.url;
                }
                else
                {
                    shop.ShopImageUrl = null;
                }

                Util.DeadlockRetry(db.SaveChanges, "Shop");
                Cache.Delete(Request.Headers.Authorization.Parameter);
                return GetShopProfile();
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopsSeller")]
        [HttpGet]
        public HttpResponseMessage GetShopSeller()
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var shop = db.Shops.Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.ShopAddress,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.Facebook,
                        s.Youtube,
                        s.Twitter,
                        s.Instagram,
                        s.Pinterest,
                        s.StockAlert
                    }).ToList();
                if (shop == null || shop.Count == 0)
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
                #region Shop Onwer
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
                #endregion

                shop = new Shop();
                SetupShopAdmin(shop, request);
                shop.Status =  Validation.ValidateString(request.Status, "Status", true, 2, false);
                shop.CreatedBy = this.User.UserRequest().Email;
                shop.CreatedDt = DateTime.Now;
                shop.UpdatedBy = this.User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                db.Shops.Add(shop);
                shop.User = usr;
                Util.DeadlockRetry(db.SaveChanges, "Shop");

                //shop.ShopOwner = usr.UserId;

                UserShop userShop = new UserShop();
                userShop.ShopId = shop.ShopId;
                userShop.UserId = usr.UserId;
                userShop.CreatedBy = this.User.UserRequest().Email;
                userShop.CreatedDt = DateTime.Now;
                userShop.UpdatedBy = this.User.UserRequest().Email;
                userShop.UpdatedDt = DateTime.Now;
                db.UserShops.Add(userShop);

                UserGroupMap map = new UserGroupMap();
                map.UserId = usr.UserId;
                map.GroupId = Constant.SHOP_OWNER_GROUP_ID;
                map.CreatedBy = this.User.UserRequest().Email;
                map.CreatedDt = DateTime.Now;
                map.UpdatedBy = this.User.UserRequest().Email;
                map.UpdatedDt = DateTime.Now;
                db.UserGroupMaps.Add(map);

                if(request.Commissions != null && request.Commissions.Count > 0)
                {
                    var catIds = request.Commissions.Where(w => w.CategoryId != null).Select(s => s.CategoryId).ToList();
                    var globalCategory = db.GlobalCategories.Where(w => catIds.Contains(w.CategoryId)).ToList();
                    foreach(var catMap in request.Commissions)
                    {
                        if(catMap.CategoryId == null && catMap.Commission == null)
                        {
                            throw new Exception("Category or commission cannot be null");
                        }
                        var category = globalCategory.Where(w => w.CategoryId == catMap.CategoryId.Value).SingleOrDefault();
                        if(category == null)
                        {
                            throw new Exception("Cannot find category " + catMap.CategoryId.Value);
                        }
                        shop.ShopCommissions.Add(new Entity.Models.ShopCommission()
                        {
                            CategoryId = catMap.CategoryId.Value,
                            Commission = catMap.Commission.Value,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }

                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                if (usr != null && usr.UserId != 0)
                {
                    db.Users.Remove(usr);
                    Util.DeadlockRetry(db.SaveChanges, "Shop");
                }
                if (shop != null && shop.ShopId != 0)
                {
                    db.Shops.Remove(shop);
                    Util.DeadlockRetry(db.SaveChanges, "Shop");
                }
                
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
                #region Query
                var shopList = db.Shops
                    .Include(i=>i.User.UserGroupMaps)
                    .Include(i=>i.UserShops)
                    .Include(i=>i.ShopCommissions)
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE)).ToList();
                if (shopList == null || shopList.Count == 0)
                {
                    throw new Exception("Shop not found");
                }
                #endregion
                shop = shopList[0];
                #region Shop Owner
                if (shop.User != null)
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
                        Util.DeadlockRetry(db.SaveChanges, "Shop");

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
                #endregion
                shop.ShopGroup = Validation.ValidateString(request.ShopGroup, "Shop Group", true, 2, false);
                shop.MaxLocalCategory = request.MaxLocalCategory;
                if (shop.MaxLocalCategory == 0)
                {
                    shop.MaxLocalCategory = Constant.MAX_LOCAL_CATEGORY;
                }
                shop.Commission = request.Commission;
                shop.ShopNameEn = Validation.ValidateString(request.ShopNameEn, "Shop Name", true, 100, false);
                shop.ShopNameTh = Validation.ValidateString(request.ShopNameTh, "Shop Name (Thai)", false, 100, false, string.Empty);
                shop.BankName = Validation.ValidateString(request.BankName, "Bank Name", true, 100, false);
                shop.BankAccountName = Validation.ValidateString(request.BankAccountName, "Bank Account Name", true, 100, false);
                shop.BankAccountNumber = Validation.ValidateString(request.BankAccountNumber, "Bank Account Number", true, 100, false);
                #region Shop Commission
                if (request.Commissions != null && request.Commissions.Count > 0)
                {
                    var catIds = request.Commissions.Where(w => w.CategoryId != null).Select(s => s.CategoryId).ToList();
                    var globalCategory = db.GlobalCategories.Where(w => catIds.Contains(w.CategoryId)).ToList();
                    var commissions = shop.ShopCommissions.ToList();
                    foreach (var catMap in request.Commissions)
                    {
                        if (catMap.CategoryId == null && catMap.Commission == null)
                        {
                            throw new Exception("Category or commission cannot be null");
                        }
                        var category = globalCategory.Where(w => w.CategoryId == catMap.CategoryId.Value).SingleOrDefault();
                        if (category == null)
                        {
                            throw new Exception("Cannot find category " + catMap.CategoryId.Value);
                        }

                        bool isNew = false;
                        if(commissions == null || commissions.Count == 0)
                        {
                            isNew = true;
                        }
                        if (!isNew)
                        {
                            var current = commissions.Where(w => w.CategoryId == catMap.CategoryId.Value).SingleOrDefault();
                            if(current != null)
                            {
                                current.Commission = catMap.Commission.Value;
                                current.CreatedBy = this.User.UserRequest().Email;
                                current.CreatedDt = DateTime.Now;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                commissions.Remove(current);
                            }
                            else
                            {
                                isNew = true;
                            }
                        }
                        if (isNew)
                        {
                            shop.ShopCommissions.Add(new Entity.Models.ShopCommission()
                            {
                                CategoryId = catMap.CategoryId.Value,
                                Commission = catMap.Commission.Value,
                                CreatedBy = this.User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = this.User.UserRequest().Email,
                                UpdatedDt = DateTime.Now,
                            });
                        }
                    }
                    if(commissions != null && commissions.Count > 0)
                    {
                        db.ShopCommissions.RemoveRange(commissions);
                    }
                }
                #endregion
                shop.Status = Validation.ValidateString(request.Status, "Shop Group", true, 2, false);
                shop.UpdatedBy = this.User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
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
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops")]
        [HttpDelete]
        public HttpResponseMessage DeleteShop(List<ShopRequest> request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopIds = request.Where(w => w.ShopId != null).Select(s => s.ShopId).ToList();
                if(shopIds == null || shopIds.Count == 0)
                {
                    throw new Exception("No shop selected");
                }
                var shops = db.Shops.Where(w => shopIds.Contains(w.ShopId));
                db.Shops.RemoveRange(shops);
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return Request.CreateResponse(HttpStatusCode.OK, "Delete successful");
                //db.Users.Remove()
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Shops/Launch")]
        [HttpGet]
        public HttpResponseMessage LaunchShop(string status)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId.Value;
                var shop = db.Shops.Where(w => w.ShopId == shopId).SingleOrDefault();
                if(shop == null)
                {
                    throw new Exception("Shop not found");
                }
                shop.Status = status;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                Cache.Delete(Request.Headers.Authorization.Parameter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        public void SetupShopAdmin(Shop shop, ShopRequest request)
        {
            shop.ShopGroup = Validation.ValidateString(request.ShopGroup, "Shop Group", true, 2, false);
            shop.ShopNameEn = Validation.ValidateString(request.ShopNameEn, "Shop Name", true, 100, false);
            shop.ShopNameTh = Validation.ValidateString(request.ShopNameTh, "Shop Name (Thai)", false, 100, false, string.Empty);
            shop.BankName = Validation.ValidateString(request.BankName, "Bank Name", true, 100, false);
            shop.BankAccountName = Validation.ValidateString(request.BankAccountName, "Bank Account Name", true, 100, false);
            shop.BankAccountNumber = Validation.ValidateString(request.BankAccountNumber, "Bank Account Number", true, 100, false);
            shop.Commission = request.Commission;
            shop.MaxLocalCategory = request.MaxLocalCategory;
            if(request.ShopType == null)
            {
                throw new Exception("Shop type cannot be null");
            }
            shop.ShopTypeId = request.ShopType.ShopTypeId;
            if (shop.MaxLocalCategory == 0)
            {
                shop.MaxLocalCategory = Constant.MAX_LOCAL_CATEGORY;
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