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


        [Route("api/Shops/{shopId}/LocalCategories")]
        [HttpGet]
        public HttpResponseMessage GetLocalCategories(int shopId)
        {
            try
            {
                var localCat = db.LocalCategories
                    .Where(w => w.ShopId == shopId && (w.Shop.Status.Equals(Constant.STATUS_ACTIVE) || w.Shop.Status.Equals(Constant.STATUS_NOT_ACTIVE))).Select(s => new
                {
                    s.CategoryId,
                    s.NameEn,
                    s.NameTh,
                    s.Lft,
                    s.Rgt,
                    s.UrlKey,
                    s.Visibility
                });
                return Request.CreateResponse(HttpStatusCode.OK, localCat);
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopImages/{themeId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadBanner([FromUri]int themeId)
        {

            var fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.SHOP_FOLDER, 1920, 1080, 1920, 1080, 5, false);
            return Request.CreateResponse(HttpStatusCode.OK, fileUpload);

        }

        [Route("api/ShopImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                var fileUpload = await Util.SetupImage(Request, AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.SHOP_FOLDER, 0, 0, int.MaxValue, int.MaxValue, 5, true);
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
                var shopList = db.Shops.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.VendorId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.ShopGroup,
                        UpdatedDt = s.UpdateOn,
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
                var shopType = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s => s.ShopType).SingleOrDefault();
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
                var shop = db.Shops.Include(i=>i.User.UserGroupMaps.Select(ug => ug.UserGroup))
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s=>new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        BankName = s.BankDetail,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.TaxPayerId,
                        s.TermPayment,
                        s.VendorTaxRate,
                        s.WithholdingTax,
                        s.Payment,
                        s.DomainName,
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
                        s.City,
                        s.Province,
                        s.District,
                        s.Country,
                        s.UrlKey,
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
                    }).SingleOrDefault();
                if(shop == null )
                {
                    throw new Exception("Cannot find shop");
                }
                return Request.CreateResponse(HttpStatusCode.OK, shop);
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
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
                    .Select(s => new
                    {
                        s.ShopId,
                        s.ShopNameEn,
                        ShopType = new { s.ShopTypeId, s.ShopType.ShopTypeNameEn },
                        s.Status,
                        s.Commission,
                        s.ShopGroup,
                        s.MaxLocalCategory,
                        BankName = s.BankDetail,
                        s.BankAccountName,
                        s.BankAccountNumber,
                        s.TaxPayerId,
                        s.TermPayment,
                        s.VendorTaxRate,
                        s.WithholdingTax,
                        s.Payment,
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
                        s.City,
                        s.Province,
                        s.District,
                        s.Country,
                        s.UrlKey,
                        s.DomainName,
                        Users = s.UserShopMaps.Select(u => u.User.Status.Equals(Constant.STATUS_REMOVE) ? null :
                        new
                        {
                            u.User.UserId,
                            u.User.NameEn,
                            u.User.Email,
                            u.User.Status,
                            u.User.Position,
                            UserGroup = u.User.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                        }),
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
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
                if (shop == null || shop.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("Cannot find shop");
                }
                SetupShopProfile(shop, request);
                shop.UpdateBy = User.UserRequest().Email;
                shop.UpdateOn = DateTime.Now;
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
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                shop = new Shop();
                SetupShopAdmin(shop, request,email, currentDt);
                shop.CreateBy = email;
                shop.CreateOn = currentDt;
                #region Owner
                User owner = new User();
                SetupUser(owner, request.ShopOwner);
                #region Password
                owner.Password = salt.HashPassword(Validation.ValidateString(request.ShopOwner.Password, "Password", true, 100, false));
                owner.PasswordLastChg = string.Empty;
                #endregion
                owner.Status = Constant.STATUS_ACTIVE;
                owner.Type = Constant.USER_TYPE_SELLER;
                owner.CreateBy = email;
                owner.CreateOn = currentDt;
                owner.UpdateBy = email;
                owner.UpdateOn = currentDt;
                owner.UserGroupMaps.Add(new UserGroupMap()
                {
                    GroupId = Constant.SHOP_OWNER_GROUP_ID,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                });
                shop.UserShopMaps.Add(new UserShopMap()
                {
                    User = owner,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                });
                shop.User = shop.UserShopMaps.ElementAt(0).User;
                #endregion
                shop.User.UserId = db.GetNextUserId().SingleOrDefault().Value;
                shop.ShopId = db.GetNextShopId().SingleOrDefault().Value;
                shop.VendorId = string.Concat(shop.ShopId);
                shop.VendorId = shop.VendorId.PadLeft(5, '0');
                string.Concat("M", shop.VendorId);
                db.Shops.Add(shop);
                Util.DeadlockRetry(db.SaveChanges, "Shop");
                return GetShop(shop.ShopId);
            }
            catch (Exception e)
            {
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
                    .Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
                if (shop == null )
                {
                    throw new Exception("Shop not found");
                }
                #endregion
                string email = User.UserRequest().Email;
                DateTime currentDt = DateTime.Now;
                SetupShopAdmin(shop, request, email, currentDt);
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
                                CreateBy = User.UserRequest().Email,
                                CreateOn = DateTime.Now,
                                UpdateBy = User.UserRequest().Email,
                                UpdateOn = DateTime.Now,
                                UserGroupMaps = new List<UserGroupMap>() { new UserGroupMap()
                        {
                            GroupId = Constant.SHOP_OWNER_GROUP_ID,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        }}
                            },
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                        shop.User = shop.UserShopMaps.ElementAt(0).User;
                        
                    }
                }
                #endregion
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
                var ids = request.Select(s => s.ShopId);
                var shopNames = db.Shops.Where(w => ids.Contains(w.ShopId)).Select(s => new
                {
                    s.ShopId,
                    s.ShopNameEn
                });
                foreach(var id in ids)
                {
                    var shop = new Shop()
                    {
                        ShopId = id,
                        Status = Constant.STATUS_REMOVE,
                        ShopOwner = null,
                        ShopNameEn = string.Concat(shopNames.Where(w => w.ShopId == id).Single().ShopNameEn, "_DELETE"),
                    };
                    db.Shops.Attach(shop);
                    db.Entry(shop).Property(p => p.Status).IsModified = true;
                    db.Entry(shop).Property(p => p.ShopOwner).IsModified = true;
                    db.Entry(shop).Property(p => p.ShopNameEn).IsModified = true;
                    db.UserShopMaps.RemoveRange(db.UserShopMaps.Where(w => w.ShopId == id));
                }
                //var shops = db.Shops.Where(w => ids.Contains(w.ShopId));
                //if(shops != null && shops.Count() > 0)
                //{
                //    shops.ToList().ForEach(e => { e.Status = Constant.STATUS_REMOVE; });
                //}
                db.Configuration.ValidateOnSaveEnabled = false;
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
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).SingleOrDefault();
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
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE))).Select(s => new
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
                if(shop.ThemeId != null)
                {
                    response.ThemeId = shop.ThemeId.Value;
                }
                
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
                var shop = db.Shops.Where(w => w.ShopId == shopId && (w.Status.Equals(Constant.STATUS_ACTIVE) || w.Status.Equals(Constant.STATUS_NOT_ACTIVE)))
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
                        compBanner.UpdateBy = User.UserRequest().Email;
                        compBanner.UpdateOn = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = banner.ComponentId,
                            IsUse = request.IsBanner,
                            Value = val,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
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
                        compLayout.UpdateBy = User.UserRequest().Email;
                        compLayout.UpdateOn = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = layout.ComponentId,
                            IsUse = request.IsLayout,
                            Value = val,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
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
                        compVideo.UpdateBy = User.UserRequest().Email;
                        compVideo.UpdateOn = DateTime.Now;
                    }
                    else
                    {
                        shop.ShopComponentMaps.Add(new ShopComponentMap()
                        {
                            ComponentId = video.ComponentId,
                            IsUse = request.IsVideo,
                            Value = val,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
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

        [Route("api/TermPayments")]
        [HttpGet]
        public HttpResponseMessage GetTermPayment()
        {
            try
            {
                var response = db.TermPayments;
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/VendorTaxRates")]
        [HttpGet]
        public HttpResponseMessage GetVendorTaxRate()
        {
            try
            {
                var response = db.VendorTaxRates.Select(s=>new
                {
                    s.Description,
                    s.VendorTaxRateCode
                }).OrderBy(o=>o.VendorTaxRateCode);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/WithholdingTaxes")]
        [HttpGet]
        public HttpResponseMessage GetWithholdingTax()
        {
            try
            {
                var response = db.WithholdingTaxes.Select(s=>new
                {
                    s.Description,
                    s.WithholdingTaxCode
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/BankNames")]
        [HttpGet]
        public HttpResponseMessage GetBankName()
        {
            try
            {
                var response = db.BankDetails.Select(s=>new
                {
                    s.BankName,
                    s.BankNumber
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Overseas")]
        [HttpGet]
        public HttpResponseMessage GetOverseas()
        {
            try
            {
                var overseasList = new List<object>()
                {
                    new
                    {
                        Key = "N",
                        Value = "No"
                    },
                    new
                    {
                            Key = "Y",
                            Value = "Yes"
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, overseasList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Countries")]
        [HttpGet]
        public HttpResponseMessage GetCountries()
        {
            try
            {
                var countryList = db.Countries.Select(s => new
                {
                    s.CountryCode,
                    s.CountryName,
                });
                return Request.CreateResponse(HttpStatusCode.OK, countryList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Provinces")]
        [HttpGet]
        public HttpResponseMessage GetProvinces()
        {
            try
            {
                var response = db.Provinces.Select(s => new
                {
                    s.ProvinceId,
                    s.ProvinceName,
                    s.ProvinceNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Cities/{provinceId}")]
        [HttpGet]
        public HttpResponseMessage GetCities([FromUri] int provinceId)
        {
            try
            {
                var response = db.Cities.Where(w=>w.ProvinceId==provinceId).Select(s=> new
                {
                    s.CityId,
                    s.CityName,
                    s.CityNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        [Route("api/Districts/{cityId}")]
        [HttpGet]
        public HttpResponseMessage GetDistricts([FromUri] int cityId)
        {
            try
            {
                var response = db.Districts.Where(w => w.CityId == cityId).Select(s => new
                {
                    s.DistrictId,
                    s.DistrictName,
                    s.DistrictNameEn
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/PostCodes/{districtId}")]
        [HttpGet]
        public HttpResponseMessage GetPostCodes([FromUri] int districtId)
        {
            try
            {
                var response = db.PostCodes.Where(w => w.DistrictId == districtId).Select(s => new
                {
                    s.PostCodeId,
                    PostCode = s.PostCode1,
                });
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }


        private void SetupShopAdmin(Shop shop, ShopRequest request,string email, DateTime currentDt)
        {
            SetupShopProfile(shop, request);
            shop.ShopGroup = Validation.ValidateString(request.ShopGroup, "Shop Group", true, 2, false, Constant.SHOP_GROUP_MERCHANT, new List<string>() { Constant.SHOP_GROUP_BU, Constant.SHOP_GROUP_INDY,Constant.SHOP_GROUP_MERCHANT });
            #region Shoptype
            if (request.ShopType == null)
            {
                throw new Exception("Shop Type is requeired");
            }
            var typeId = db.ShopTypes.Where(w => w.ShopTypeId == request.ShopType.ShopTypeId).Select(s => s.ShopTypeId).SingleOrDefault();
            if (typeId == 0)
            {
                throw new Exception("Cannot find Shop Type");
            }
            shop.ShopTypeId = typeId;
            #endregion
            shop.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.STATUS_NOT_ACTIVE, new List<string>() { Constant.STATUS_ACTIVE, Constant.STATUS_NOT_ACTIVE });
            shop.MaxLocalCategory = request.MaxLocalCategory;
            if (shop.MaxLocalCategory == 0)
            {
                shop.MaxLocalCategory = Constant.MAX_LOCAL_CATEGORY;
            }
            shop.Commission = request.Commission;
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
                    if (commissions == null || commissions.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = commissions.Where(w => w.CategoryId == catMap.CategoryId).SingleOrDefault();
                        if (current != null)
                        {
                            current.Commission = catMap.Commission;
                            current.UpdateBy = email;
                            current.UpdateOn = currentDt;
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
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
            }
            if (commissions != null && commissions.Count > 0)
            {
                db.ShopCommissions.RemoveRange(commissions);
            }
            #endregion
            shop.UrlKey = Validation.ValidateString(request.UrlKeyEn, "Url Key (English)", true, 100, false, shop.ShopNameEn.Replace(" ","-"));
            shop.TaxPayerId = Validation.ValidateString(request.TaxPayerId, "Tax Payer Id", true, 35, false, string.Empty);
            if(request.TermPayment != null && !string.IsNullOrEmpty(request.TermPayment.TermPaymentCode))
            {
                var termCode = db.TermPayments
                    .Where(w => w.TermPaymentCode.Equals(request.TermPayment.TermPaymentCode))
                    .Select(s => s.TermPaymentCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(termCode))
                {
                    throw new Exception("Invalid Term Payment Code");
                }
                shop.TermPaymentCode = termCode;
            }
            shop.Payment = Validation.ValidateString(request.Payment, "Payment", true, 1, false);
            if (request.VendorTaxRate != null && !string.IsNullOrEmpty(request.VendorTaxRate.VendorTaxRateCode))
            {
                var code = db.VendorTaxRates
                    .Where(w => w.VendorTaxRateCode.Equals(request.VendorTaxRate.VendorTaxRateCode))
                    .Select(s => s.VendorTaxRateCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Vendor Tax Rate Code");
                }
                shop.VendorTaxRateCode = code;
            }
            if (request.WithholdingTax != null && !string.IsNullOrEmpty(request.WithholdingTax.WithholdingTaxCode))
            {
                var code = db.WithholdingTaxes
                    .Where(w => w.WithholdingTaxCode.Equals(request.WithholdingTax.WithholdingTaxCode))
                    .Select(s => s.WithholdingTaxCode)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Withholding Tax Code");
                }
                shop.WithholdingTaxCode = code;
            }
            if (request.BankName != null && !string.IsNullOrEmpty(request.BankName.BankNumber))
            {
                var code = db.BankDetails
                    .Where(w => w.BankNumber.Equals(request.BankName.BankNumber))
                    .Select(s => s.BankNumber)
                    .SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Invalid Bank Name");
                }
                shop.BankNumber = code;
            }
            shop.BankAccountNumber = Validation.ValidateString(request.BankAccountNumber, "Bank Account Number", true, 100, false);
            shop.BankAccountName = Validation.ValidateString(request.BankAccountName, "Bank Account Name", true, 100, false);
            shop.UpdateBy = email;
            shop.UpdateOn = currentDt;
        }

        private void SetupShopProfile(Shop shop, ShopRequest request)
        {
            shop.ShopNameEn = Validation.ValidateString(request.ShopNameEn, "Shop Name", true, 100, true);
            shop.ShopNameTh = Validation.ValidateString(request.ShopNameTh, "Shop Name (Thai)", false, 100, false, string.Empty);
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

            shop.VendorAddressLine1 = Validation.ValidateString(request.VendorAddressLine1, "Vendor Address Line1", true, 35, false, string.Empty);
            shop.VendorAddressLine2 = Validation.ValidateString(request.VendorAddressLine2, "Vendor Address Line2", true, 35, false, string.Empty);
            shop.VendorAddressLine3 = Validation.ValidateString(request.VendorAddressLine3, "Vendor Address Line3", true, 35, false, string.Empty);
            shop.OverseasVendorIndicator = Validation.ValidateString(request.OverseasVendorIndicator, "Overseas Vendor Indicator", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
            shop.DomainName = Validation.ValidateString(request.DomainName, "Domain Name", true, 500, false, string.Empty);

            if (request.Country != null && !string.IsNullOrEmpty(request.Country.CountryCode))
            {
                var id = db.Countries.Where(w => w.CountryCode.Equals(request.Country.CountryCode)).Select(s => s.CountryCode).SingleOrDefault();
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Cannot find city");
                }
                shop.CountryCode = id;
            }


            if (request.Province != null && request.Province.ProvinceId != 0)
            {
                var provinceId = db.Provinces.Where(w => w.ProvinceId == request.Province.ProvinceId).Select(s => s.ProvinceId).SingleOrDefault();
                if (provinceId == 0)
                {
                    throw new Exception("Cannot find Province");
                }
                shop.ProvinceId = provinceId;

                if (request.City != null && request.City.CityId != 0)
                {
                    var cityId = db.Cities.Where(w => w.ProvinceId == shop.ProvinceId && w.CityId == request.City.CityId).Select(s => s.CityId).SingleOrDefault();
                    if (cityId == 0)
                    {
                        throw new Exception("Cannot find City");
                    }
                    shop.CityId = cityId;

                    if (request.District != null && request.District.DistrictId != 0)
                    {
                        var districtId = db.Districts.Where(w => w.CityId == shop.CityId && w.DistrictId == request.District.DistrictId).Select(s => s.CityId).SingleOrDefault();
                        if (districtId == 0)
                        {
                            throw new Exception("Cannot find District");
                        }
                        shop.DistrictId = districtId;
                    }
                }
            }
            shop.ZipCode = Validation.ValidateString(request.ZipCode, "Zip Code", true, 10, false, string.Empty);
            shop.PhoneNumber = Validation.ValidateString(request.PhoneNumber, "Phone Number", true, 15, false, string.Empty);
            shop.FaxNumber = Validation.ValidateString(request.FaxNumber, "Fax Number", true, 15, false, string.Empty);
            shop.Telex = Validation.ValidateString(request.Telex, "Telex", true, 15, false, string.Empty);
            shop.OverseasVendorIndicator = Validation.ValidateString(request.OverseasVendorIndicator, "Overseas Vendor Indicator", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_NO, Constant.STATUS_YES });
            shop.RemittanceFaxNumber = Validation.ValidateString(request.Telex, "Remittance Fax Number", true, 18, false, string.Empty);
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