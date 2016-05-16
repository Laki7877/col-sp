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
                        Permission = s.ShopTypePermissionMaps.Select(p => new { p.Permission.PermissionId, p.Permission.PermissionName, p.Permission.PermissionGroup })
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
                shopType = new ShopType();
                shopType.ShopTypeNameEn = request.ShopTypeNameEn;
                shopType.Status = Constant.STATUS_ACTIVE;
                shopType.CreateBy = User.UserRequest().Email;
                shopType.CreateOn = DateTime.Now;
                shopType.UpdateBy = User.UserRequest().Email;
                shopType.UpdateOn = DateTime.Now;
                
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
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
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
                shopType.ShopTypeNameEn = request.ShopTypeNameEn;
                var mapList = db.ShopTypePermissionMaps.Where(w => w.ShopTypeId == shopType.ShopTypeId).ToList();
                if (request.Permission != null && request.Permission.Count > 0)
                {
                    bool addNew = false;
                    foreach (PermissionRequest permission in request.Permission)
                    {
                        if (mapList == null || mapList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapList.Where(w => w.PermissionId == permission.PermissionId).SingleOrDefault();
                            if (current != null)
                            {
                                current.UpdateBy = User.UserRequest().Email;
                                current.UpdateOn = DateTime.Now;
                                mapList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            ShopTypePermissionMap map = new ShopTypePermissionMap();
                            map.ShopTypeId = shopType.ShopTypeId;
                            map.PermissionId = permission.PermissionId.Value;
                            map.CreateBy = User.UserRequest().Email;
                            map.CreateOn = DateTime.Now;
                            map.UpdateBy = User.UserRequest().Email;
                            map.UpdateOn = DateTime.Now;
                            db.ShopTypePermissionMaps.Add(map);
                        }
                    }
                }
                if (mapList != null && mapList.Count > 0)
                {
                    db.ShopTypePermissionMaps.RemoveRange(mapList);
                }
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