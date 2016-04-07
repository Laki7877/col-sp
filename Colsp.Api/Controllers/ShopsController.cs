using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Helpers;
using System.Threading.Tasks;
using Cenergy.Dazzle.Admin.Security.Cryptography;
using System.Web.Script.Serialization;

namespace Colsp.Api.Controllers
{
    public class ShopsController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

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
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
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
                    || a.ShopId.ToString().Equals(request.SearchText)
                    || a.ShopType.ShopTypeNameEn.Contains(request.SearchText));
                }
                var total = shopList.Count();
                var pagedUsers = shopList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/{shopId}")]
        [HttpGet]
        public HttpResponseMessage GetShop(int shopId)
        {
            try
            {
                var shop = db.Shops.Include(i=>i.User.UserGroupMaps.Select(ug => ug.UserGroup)).Include(i=>i.ShopType)
                    .Include(i=>i.UserShopMaps.Select(s=>s.User.UserGroupMaps.Select(ug=>ug.UserGroup)))
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s=>new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        s.BankName,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        Commissions = s.ShopCommissions.Select(sc => new { sc.CategoryId, sc.Commission }),
                        ShopOwner = new
                        {
                            s.User.UserId,
                            s.User.NameEn,
                            s.User.Email,
                            s.User.Phone,
                            s.User.Status,
                            s.User.Position,
                            UserGroup = s.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        },
                        ShopImage = new ImageRequest { Url = s.ShopImageUrl },
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.FloatMessageEn,
                        s.FloatMessageTh,
                        s.ShopAddress,
                        s.Facebook,
                        s.YouTube,
                        s.Twitter,
                        s.Instagram,
                        s.Pinterest,
                        s.GiftWrap,
                        s.TaxInvoice,
                        s.StockAlert,
                        Users = s.UserShopMaps.Select(u=> u.User.Status.Equals(Constant.STATUS_REMOVE) ? null :
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Profile")]
        [HttpGet]
        public HttpResponseMessage GetShopProfile()
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var shop = db.Shops
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        s.BankName,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        Commissions = s.ShopCommissions.Select(sc => new { sc.CategoryId, sc.Commission }),
                        ShopOwner = new
                        {
                            s.User.UserId,
                            s.User.NameEn,
                            s.User.Email,
                            s.User.Phone,
                            s.User.Status,
                            s.User.Position,
                            UserGroup = s.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        },
                        ShopImage = new ImageRequest { Url = s.ShopImageUrl },
                        s.ShopDescriptionEn,
                        s.ShopDescriptionTh,
                        s.FloatMessageEn,
                        s.FloatMessageTh,
                        s.ShopAddress,
                        s.Facebook,
                        s.YouTube,
                        s.Twitter,
                        s.Instagram,
                        s.Pinterest,
                        s.GiftWrap,
                        s.TaxInvoice,
                        s.StockAlert
                    }).SingleOrDefault();
                if (shop == null)
                {
                    throw new Exception("Cannot find shop");
                }
                return Request.CreateResponse(HttpStatusCode.OK,shop);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Profile")]
        [HttpPut]
        public HttpResponseMessage SaveShopProfile(ShopRequest request)
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE)).SingleOrDefault();
                if (shop == null || shop.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("Cannot find shop");
                }
                SetupShopProfile(shop, request);
                shop.UpdatedBy = User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShopProfile();
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops")]
        [HttpPost]
        public HttpResponseMessage AddShop(ShopRequest request)
        {
            Shop shop = null;
            try
            {
                shop = new Shop()
                {
                    ThemeId = Constant.DEFAULT_THEME_ID,
                };
                SetupShopAdmin(shop, request);
                shop.CreatedBy = User.UserRequest().Email;
                shop.CreatedDt = DateTime.Now;
                shop.UpdatedBy = User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                db.Shops.Add(shop);
                #region Owner
                User owner = new User();
                SetupUser(owner, request.ShopOwner);
                #region Password
                owner.Password = salt.HashPassword(Validation.ValidateString(request.ShopOwner.Password, "Password", true, 100, false));
                owner.PasswordLastChg = string.Empty;
                #endregion
                owner.Status = Constant.STATUS_ACTIVE;
                owner.Type = Constant.USER_TYPE_SELLER;
                owner.CreatedBy = User.UserRequest().Email;
                owner.CreatedDt = DateTime.Now;
                owner.UpdatedBy = User.UserRequest().Email;
                owner.UpdatedDt = DateTime.Now;
                owner.UserGroupMaps.Add(new UserGroupMap()
                {
                    GroupId = Constant.SHOP_OWNER_GROUP_ID,
                    CreatedBy = User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = User.UserRequest().Email,
                    UpdatedDt = DateTime.Now,
                });
                shop.UserShopMaps.Add(new UserShopMap()
                {
                    User = owner,
                    CreatedBy = User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = User.UserRequest().Email,
                    UpdatedDt = DateTime.Now,
                });
                shop.User = shop.UserShopMaps.ElementAt(0).User;
                #endregion
                #region Commission
                if (request.Commissions != null && request.Commissions.Count > 0)
                {
                    var catIds = request.Commissions.Where(w => w.CategoryId != 0).Select(s => s.CategoryId).ToList();
                    var globalCategory = db.GlobalCategories.Where(w => catIds.Contains(w.CategoryId)).ToList();
                    foreach(var catMap in request.Commissions)
                    {
                        if(catMap.CategoryId == 0 && catMap.Commission == 0)
                        {
                            throw new Exception("Category or commission cannot be null");
                        }
                        var category = globalCategory.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                        if(category == null)
                        {
                            throw new Exception("Cannot find category " + catMap.CategoryId);
                        }
                        shop.ShopCommissions.Add(new Entity.Models.ShopCommission()
                        {
                            CategoryId = catMap.CategoryId,
                            Commission = catMap.Commission,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                #endregion 
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                #region Rollback
                if (shop != null && shop.ShopId != 0)
                {
                    db.Shops.Remove(shop);
                    Util.DeadlockRetry(db.SaveChanges, "Shop");
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/{shopId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeShop([FromUri]int shopId, ShopRequest request)
        {
            Shop shop = null;
            try
            {
                #region Query
                shop = db.Shops
                    .Include(i=>i.User.UserGroupMaps)
                    .Include(i=>i.UserShopMaps)
                    .Include(i=>i.ShopCommissions)
                    .Where(w => w.ShopId == shopId && !w.Status.Equals(Constant.STATUS_REMOVE)).SingleOrDefault();
                if (shop == null )
                {
                    throw new Exception("Shop not found");
                }
                #endregion
                SetupShopAdmin(shop, request);
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
                        db.UserShopMaps.RemoveRange(shop.UserShopMaps);

                        shop.UserShopMaps.Add(new UserShopMap()
                        {
                            User = new Entity.Models.User()
                            {
                                Email = Validation.ValidateString(request.ShopOwner.Email, "Email", true, 100, false),
                                Password = request.ShopOwner.Password,
                                NameEn = request.ShopOwner.NameEn,
                                NameTh = request.ShopOwner.NameTh,
                                Division = request.ShopOwner.Division,
                                Phone = request.ShopOwner.Phone,
                                Position = request.ShopOwner.Position,
                                EmployeeId = request.ShopOwner.EmployeeId,
                                Status = Constant.STATUS_ACTIVE,
                                Type = Constant.USER_TYPE_SELLER,
                                CreatedBy = User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = User.UserRequest().Email,
                                UpdatedDt = DateTime.Now,
                                UserGroupMaps = new List<UserGroupMap>() { new UserGroupMap()
                        {
                            GroupId = Constant.SHOP_OWNER_GROUP_ID,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        }}
                            },
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                        shop.User = shop.UserShopMaps.ElementAt(0).User;
                        
                    }
                }
                #endregion
                #region Shop Commission
                var commissions = shop.ShopCommissions.ToList();
                if (request.Commissions != null && request.Commissions.Count > 0)
                {
                    var catIds = request.Commissions.Where(w => w.CategoryId != 0).Select(s => s.CategoryId).ToList();
                    var globalCategory = db.GlobalCategories.Where(w => catIds.Contains(w.CategoryId)).ToList();
                    foreach (var catMap in request.Commissions)
                    {
                        if (catMap.CategoryId == 0 && catMap.Commission == 0)
                        {
                            throw new Exception("Category or commission cannot be null");
                        }
                        var category = globalCategory.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                        if (category == null)
                        {
                            throw new Exception("Cannot find category " + catMap.CategoryId);
                        }

                        bool isNew = false;
                        if(commissions == null || commissions.Count == 0)
                        {
                            isNew = true;
                        }
                        if (!isNew)
                        {
                            var current = commissions.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                            if(current != null)
                            {
                                current.Commission = catMap.Commission;
                                current.CreatedBy = User.UserRequest().Email;
                                current.CreatedDt = DateTime.Now;
                                current.UpdatedBy = User.UserRequest().Email;
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
                                CategoryId = catMap.CategoryId,
                                Commission = catMap.Commission,
                                CreatedBy = User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = User.UserRequest().Email,
                                UpdatedDt = DateTime.Now,
                            });
                        }
                    }
                }
                if (commissions != null && commissions.Count > 0)
                {
                    db.ShopCommissions.RemoveRange(commissions);
                }
                #endregion
                shop.UpdatedBy = User.UserRequest().Email;
                shop.UpdatedDt = DateTime.Now;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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
                var shopIds = request.Where(w => w.ShopId != 0).Select(s => s.ShopId).ToList();
                if(shopIds == null || shopIds.Count == 0)
                {
                    throw new Exception("No shop selected");
                }
                var shops = db.Shops.Where(w => shopIds.Contains(w.ShopId));
                db.Shops.RemoveRange(shops);
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return Request.CreateResponse(HttpStatusCode.OK, "Delete successful");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/Launch")]
        [HttpGet]
        public HttpResponseMessage LaunchShop(string status)
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId).SingleOrDefault();
                if(shop == null)
                {
                    throw new Exception("Shop not found");
                }
                shop.Status = status;
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                User.ShopRequest().Status = status;
                //Cache.Delete(Request.Headers.Authorization.Parameter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Shops/ShopAppearance")]
        [HttpGet]
        public HttpResponseMessage GetShopAppearance()
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId).Select(s => new
                {
                    s.ShopId,
                    s.ThemeId,
                    ShopComponentMaps = s.ShopComponentMaps.Select(sm=>new
                    {
                        sm.IsUse,
                        sm.Value,
                        ThemeComponent = sm.ThemeComponent == null ? null : new
                        {
                            sm.ThemeComponent.ComponentId,
                            sm.ThemeComponent.ComponentName
                        },
                    }),
                }).SingleOrDefault();
                if(shop == null)
                {
                    throw new Exception("Cannot find shop");
                }
                ShopAppearanceRequest response = new ShopAppearanceRequest();
                if(shop.ShopComponentMaps != null)
                {
                    foreach (var component in shop.ShopComponentMaps)
                    {
                        if (component.ThemeComponent == null)
                        {
                            continue;
                        }

                        if ("Banner".Equals(component.ThemeComponent.ComponentName))
                        {
                            response.IsBanner = component.IsUse;
                            response.Banner = new JavaScriptSerializer().Deserialize<BannerComponent>(component.Value);
                        }
                        else if ("Layout".Equals(component.ThemeComponent.ComponentName))
                        {
                            response.IsLayout = component.IsUse;
                            response.Layouts = new JavaScriptSerializer().Deserialize<List<Layout>>(component.Value);
                        }
                        else if ("Video".Equals(component.ThemeComponent.ComponentName))
                        {
                            response.IsVideo = component.IsUse;
                            response.Videos = new JavaScriptSerializer().Deserialize<List<VideoLinkRequest>>(component.Value);
                        }
                    }
                }
                response.ThemeId = shop.ThemeId;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Shops/ShopAppearance")]
        [HttpPut]
        public HttpResponseMessage SaveShopAppearance(ShopAppearanceRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }

                var shopId = User.ShopRequest().ShopId;
                var shop = db.Shops.Where(w => w.ShopId == shopId)
                    .Include(i => i.ShopComponentMaps)
                    .SingleOrDefault();

                var components = db.ThemeComponents.Select(s => new
                {
                    s.ComponentId,
                    s.ComponentName
                });


                shop.ThemeId = request.ThemeId;

                var banner = components.Where(w => w.ComponentName.Equals("Banner")).SingleOrDefault();
                var compBanner = shop.ShopComponentMaps.Where(w => w.ComponentId == banner.ComponentId).SingleOrDefault();
                if (request.Banner != null)
                {
                    string val = new JavaScriptSerializer().Serialize(request.Banner);
                    if (compBanner != null)
                    {
                        compBanner.IsUse = request.IsBanner;
                        compBanner.Value = val;
                        compBanner.UpdatedBy = User.UserRequest().Email;
                        compBanner.UpdatedDt = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = banner.ComponentId,
                            IsUse = request.IsBanner,
                            Value = val,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                else if(compBanner != null)
                {
                    db.ShopComponentMaps.Remove(compBanner);
                }

                var layout = components.Where(w => w.ComponentName.Equals("Layout")).SingleOrDefault();
                var compLayout = shop.ShopComponentMaps.Where(w => w.ComponentId == layout.ComponentId).SingleOrDefault();
                if (request.Layouts != null)
                {
                    string val = new JavaScriptSerializer().Serialize(request.Layouts);
                    if (compLayout != null)
                    {
                        compLayout.IsUse = request.IsLayout;
                        compLayout.Value = val;
                        compLayout.UpdatedBy = User.UserRequest().Email;
                        compLayout.UpdatedDt = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = layout.ComponentId,
                            IsUse = request.IsLayout,
                            Value = val,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                else if (compLayout != null)
                {
                    db.ShopComponentMaps.Remove(compLayout);
                }

                var video = components.Where(w => w.ComponentName.Equals("Video")).SingleOrDefault();
                var compVideo = shop.ShopComponentMaps.Where(w => w.ComponentId == video.ComponentId).SingleOrDefault();
                if (request.Videos != null)
                {
                    string val = new JavaScriptSerializer().Serialize(request.Videos);
                    if (compVideo != null)
                    {
                        compVideo.IsUse = request.IsVideo;
                        compVideo.Value = val;
                        compVideo.UpdatedBy = User.UserRequest().Email;
                        compVideo.UpdatedDt = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = video.ComponentId,
                            IsUse = request.IsVideo,
                            Value = val,
                            CreatedBy = User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                else if (compVideo != null)
                {
                    db.ShopComponentMaps.Remove(compVideo);
                }

                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShopAppearance();
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        private void SetupShopAdmin(Shop shop, ShopRequest request)
        {
            SetupShopProfile(shop, request);
            shop.ShopGroup = Validation.ValidateString(request.ShopGroup, "Shop Group", true, 2, false, Constant.SHOP_GROUP_MERCHANT, new List<string>() { Constant.SHOP_GROUP_BU, Constant.SHOP_GROUP_INDY,Constant.SHOP_GROUP_MERCHANT });
            shop.ShopNameTh = Validation.ValidateString(request.ShopNameTh, "Shop Name (Thai)", false, 100, false, string.Empty);
            shop.BankName = Validation.ValidateString(request.BankName, "Bank Name", true, 100, false);
            shop.BankAccountName = Validation.ValidateString(request.BankAccountName, "Bank Account Name", true, 100, false);
            shop.BankAccountNumber = Validation.ValidateString(request.BankAccountNumber, "Bank Account Number", true, 100, false);
            shop.Commission = request.Commission;
            shop.UrlKeyEn = Validation.ValidateString(request.UrlKeyEn, "Url Key (English)", true, 100, false, shop.ShopNameEn.Replace(" ","-"));
            if (request.ShopType == null)
            {
                throw new Exception("Shop type cannot be null");
            }
            shop.ShopTypeId = request.ShopType.ShopTypeId;
            shop.MaxLocalCategory = request.MaxLocalCategory;
            if (shop.MaxLocalCategory == 0)
            {
                shop.MaxLocalCategory = Constant.MAX_LOCAL_CATEGORY;
            }

            shop.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.STATUS_ACTIVE, Constant.STATUS_NOT_ACTIVE });
        }

        private void SetupShopProfile(Shop shop, ShopRequest request)
        {
            shop.ShopNameEn = Validation.ValidateString(request.ShopNameEn, "Shop Name", true, 100, true);
            shop.ShopImageUrl = string.Empty;
            if (request.ShopImage != null)
            {
                shop.ShopImageUrl = Validation.ValidateString(request.ShopImage.Url, "Image", false, 1000, false, string.Empty);
            }
            shop.ShopDescriptionEn = Validation.ValidateString(request.ShopDescriptionEn, "Shop Description (English)", false, 500, false, string.Empty);
            shop.ShopDescriptionTh = Validation.ValidateString(request.ShopDescriptionTh, "Shop Description (Thai)", false, 500, false, string.Empty);
            shop.FloatMessageEn = Validation.ValidateString(request.FloatMessageEn, "Float Message (English)", false, 100, false, string.Empty);
            shop.FloatMessageTh = Validation.ValidateString(request.FloatMessageTh, "Float Message (Thai)", false, 100, false, string.Empty);
            shop.ShopAddress = Validation.ValidateString(request.ShopAddress, "Shop Address", false, 500, false, string.Empty);
            shop.Facebook = Validation.ValidateString(request.Facebook, "Facebook", false, 500, false, string.Empty);
            shop.YouTube = Validation.ValidateString(request.YouTube, "YouTube", false, 500, false, string.Empty);
            shop.Instagram = Validation.ValidateString(request.Instagram, "Instagram", false, 500, false, string.Empty);
            shop.Pinterest = Validation.ValidateString(request.Pinterest, "Pinterest", false, 500, false, string.Empty);
            shop.Twitter = Validation.ValidateString(request.Twitter, "Twitter", false, 500, false, string.Empty);
            shop.GiftWrap = Validation.ValidateString(request.GiftWrap, "Gift Wrap", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            shop.TaxInvoice = Validation.ValidateString(request.TaxInvoice, "Tax Invoice", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            shop.StockAlert = Validation.ValidationInteger(request.StockAlert, "Stock Alert", true, int.MaxValue, 0).Value;
            shop.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.STATUS_NOT_ACTIVE, new List<string>() { Constant.STATUS_NOT_ACTIVE, Constant.STATUS_ACTIVE});
        }

        private void SetupUser(User user, UserRequest request)
        {
            user.Email = Validation.ValidateString(request.Email, "Email", true, 100, false);
            user.NameEn = Validation.ValidateString(request.NameEn, "Name", true, 100, false);
            user.NameTh = Validation.ValidateString(request.NameTh, "Name", false, 100, false, string.Empty);
            user.Division = Validation.ValidateString(request.Division, "Division", false, 100, false, string.Empty);
            user.Phone = Validation.ValidateString(request.Phone, "Phone", true, 100, false);
            user.Position = Validation.ValidateString(request.Position, "Position", false, 100, false, string.Empty);
            user.EmployeeId = Validation.ValidateString(request.EmployeeId, "Employee Id", false, 100, false, string.Empty);
            user.Mobile = Validation.ValidateString(request.Mobile, "Mobile", false, 20, false, string.Empty);
            user.Fax = Validation.ValidateString(request.Fax, "Fax", false, 20, false, string.Empty);
        }

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