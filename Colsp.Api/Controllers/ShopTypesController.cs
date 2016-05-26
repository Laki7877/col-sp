using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Constants;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class ShopTypesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ShopTypes")]
        [HttpGet]
        public HttpResponseMessage GetShopType([FromUri] ShopTypeRequest request)
        {
            try
            {
                var shopTypeList = db.ShopTypes.Include(i=>i.Shops).Where(w=>!w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopTypeId,
                        s.ShopTypeNameEn,
                        Permission = s.ShopTypePermissionMaps.Select(p=> new { p.Permission.PermissionId,p.Permission.PermissionName,p.Permission.PermissionGroup}),
                        UpdatedDt = s.UpdateOn,
                        ShopCount = s.Shops.Count
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, shopTypeList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    shopTypeList = shopTypeList.Where(w => w.ShopTypeNameEn.Contains(request.SearchText));
                }
                var total = shopTypeList.Count();
                var pagedUsers = shopTypeList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopTypes/{shopTypeid}")]
        [HttpGet]
        public HttpResponseMessage GetShopType(int shopTypeid)
        {
            try
            {
                var shopTypeList = db.ShopTypes
                    .Where(w => w.ShopTypeId == shopTypeid&& !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.ShopTypeId,
                        s.ShopTypeNameEn,
                        Permission = s.ShopTypePermissionMaps.Select(p => new
                        {
                            p.Permission.PermissionId,
                            p.Permission.PermissionName,
                            p.Permission.PermissionGroup
                        }),
                        Themes = s.ShopTypeThemeMaps.Select(t => new
                        {
                            t.Theme.ThemeId,
                            t.Theme.ThemeName,
                            t.Theme.ThemeImage,
                        }),
                        Shippings = s.ShopTypeShippingMaps.Select(sh => new
                        {
                            sh.Shipping.ShippingId,
                            sh.Shipping.ShippingMethodEn,
                        })
                    }).ToList();
                if(shopTypeList == null || shopTypeList.Count == 0)
                {
                    throw new Exception("Shop type not found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, shopTypeList[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopTypes")]
        [HttpPost]
        public HttpResponseMessage AddShopType(ShopTypeRequest request)
        {
            ShopType shopType = null;
            try
            {
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                shopType = new ShopType();
                shopType.ShopTypeNameEn = request.ShopTypeNameEn;
                shopType.Status = Constant.STATUS_ACTIVE;
                shopType.CreateBy = email;
                shopType.CreateOn = currentDt;
                shopType.UpdateBy = email;
                shopType.UpdateOn = currentDt;
                
                if (request.Permission != null)
                {
                    foreach (PermissionRequest perm in request.Permission)
                    {
                        if (perm.PermissionId == null)
                        {
                            throw new Exception("Permission id is null");
                        }
                        shopType.ShopTypePermissionMaps.Add(new ShopTypePermissionMap()
                        {
                            PermissionId = perm.PermissionId.Value,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
                if(request.Themes != null)
                {
                    foreach(var theme in request.Themes)
                    {
                        if(theme.ThemeId != 0)
                        {
                            throw new Exception("Theme Id is null");
                        }
                        shopType.ShopTypeThemeMaps.Add(new ShopTypeThemeMap()
                        {
                            ThemeId = theme.ThemeId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
                if(request.Shippings != null)
                {
                    foreach (var shipping in request.Shippings)
                    {
                        if (shipping.ShippingId != 0)
                        {
                            throw new Exception("Shop Id is null");
                        }
                        shopType.ShopTypeShippingMaps.Add(new ShopTypeShippingMap()
                        {
                            ShippingId = shipping.ShippingId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
                db.ShopTypes.Add(shopType);
                Util.DeadlockRetry(db.SaveChanges, "ShopType");
                return GetShopType(shopType.ShopTypeId);
            }
            catch (Exception e)
            {
                if (shopType != null && shopType.ShopTypeId != 0)
                {
                    db.ShopTypes.Remove(shopType);
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopTypes/{shopTypeid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeShopType([FromUri]int shopTypeid, ShopTypeRequest request)
        {
            try
            {
                var shopType = db.ShopTypes.Find(shopTypeid);
                if (shopType == null)
                {
                    throw new Exception("User group not found");
                }
                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                shopType.ShopTypeNameEn = request.ShopTypeNameEn;
                #region Permission
                var mapPermissionList = db.ShopTypePermissionMaps.Where(w => w.ShopTypeId == shopType.ShopTypeId).ToList();
                if (request.Permission != null && request.Permission.Count > 0)
                {
                    bool addNew = false;
                    foreach (PermissionRequest permission in request.Permission)
                    {
                        if (mapPermissionList == null || mapPermissionList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapPermissionList.Where(w => w.PermissionId == permission.PermissionId).SingleOrDefault();
                            if (current != null)
                            {
                                mapPermissionList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            shopType.ShopTypePermissionMaps.Add(new ShopTypePermissionMap()
                            {
                                PermissionId = permission.PermissionId.Value,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt,
                            });
                        }
                    }
                }
                if (mapPermissionList != null && mapPermissionList.Count > 0)
                {
                    db.ShopTypePermissionMaps.RemoveRange(mapPermissionList);
                }
                #endregion
                #region Theme
                var mapThemeList = db.ShopTypeThemeMaps.Where(w => w.ShopTypeId == shopType.ShopTypeId).ToList();
                if (request.Themes != null && request.Themes.Count > 0)
                {
                    bool addNew = false;
                    foreach (var theme in request.Themes)
                    {
                        if (mapThemeList == null || mapThemeList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapThemeList.Where(w => w.ThemeId == theme.ThemeId).SingleOrDefault();
                            if (current != null)
                            {
                                mapThemeList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            shopType.ShopTypeThemeMaps.Add(new ShopTypeThemeMap()
                            {
                                ThemeId = theme.ThemeId,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt,
                            });
                        }
                    }
                }
                if (mapThemeList != null && mapThemeList.Count > 0)
                {
                    db.ShopTypeThemeMaps.RemoveRange(mapThemeList);
                }
                #endregion
                #region Shipping
                var mapShipList = db.ShopTypeShippingMaps.Where(w => w.ShopTypeId == shopType.ShopTypeId).ToList();
                if (request.Shippings != null && request.Shippings.Count > 0)
                {
                    bool addNew = false;
                    foreach (var ship in request.Shippings)
                    {
                        if (mapShipList == null || mapShipList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapShipList.Where(w => w.ShippingId == ship.ShippingId).SingleOrDefault();
                            if (current != null)
                            {
                                mapShipList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            shopType.ShopTypeShippingMaps.Add(new ShopTypeShippingMap()
                            {
                                ShippingId = ship.ShippingId,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt,
                            });
                        }
                    }
                }
                if (mapShipList != null && mapShipList.Count > 0)
                {
                    db.ShopTypeShippingMaps.RemoveRange(mapShipList);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "ShopType");
                return GetShopType(shopType.ShopTypeId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ShopTypes")]
        [HttpDelete]
        public HttpResponseMessage DeleteUserShopType(List<ShopTypeRequest> request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopTypeIds = request.Where(w => w.ShopTypeId != 0).Select(s => s.ShopTypeId).ToList();
                var shopTypes = db.ShopTypes.Where(w => shopTypeIds.Contains(w.ShopTypeId)).ToList();
                if(shopTypes == null || shopTypes.Count == 0)
                {
                    throw new Exception("No deleted shop type found");
                }
                db.ShopTypes.RemoveRange(shopTypes);
                Util.DeadlockRetry(db.SaveChanges, "ShopType");
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

        private bool ShopTypeExists(int id)
        {
            return db.ShopTypes.Count(e => e.ShopTypeId == id) > 0;
        }
    }
}