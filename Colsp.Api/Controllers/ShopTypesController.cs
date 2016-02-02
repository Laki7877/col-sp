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
using Colsp.Model.Responses;
using Colsp.Api.Constants;

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
                        s.ShopTypeNameTh,
                        Permission = s.ShopTypePermissionMaps.Select(p=> new { p.Permission.PermissionId,p.Permission.PermissionName,p.Permission.PermissionGroup}),
                        s.UpdatedDt,
                        ShopCount = s.Shops.Count
                    }).OrderBy(o=>o.ShopTypeId);
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, shopTypeList);
                }
                request.DefaultOnNull();
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
                        s.ShopTypeNameTh,
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
            ShopType ShopType = null;
            try
            {
                ShopType = new ShopType();
                ShopType.ShopTypeNameEn = request.ShopTypeNameEn;
                ShopType.Status = Constant.STATUS_ACTIVE;
                ShopType.CreatedBy = this.User.UserRequest().Email;
                ShopType.CreatedDt = DateTime.Now;
                ShopType.UpdatedBy = this.User.UserRequest().Email;
                ShopType.UpdatedDt = DateTime.Now;
                db.ShopTypes.Add(ShopType);
                db.SaveChanges();
                if (request.Permission != null)
                {
                    foreach (PermissionRequest perm in request.Permission)
                    {
                        if (perm.PermissionId == null)
                        {
                            throw new Exception("Permission id is null");
                        }
                        ShopTypePermissionMap map = new ShopTypePermissionMap();
                        map.PermissionId = perm.PermissionId.Value;
                        map.ShopTypeId = ShopType.ShopTypeId;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        db.ShopTypePermissionMaps.Add(map);
                    }
                    db.SaveChanges();
                }
                return GetShopType(ShopType.ShopTypeId);
            }
            catch (Exception e)
            {
                if (ShopType != null && ShopType.ShopTypeId != 0)
                {
                    db.ShopTypes.Remove(ShopType);
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
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.ShopTypePermissionMaps.Add(map);
                        }
                    }
                }
                if (mapList != null && mapList.Count > 0)
                {
                    db.ShopTypePermissionMaps.RemoveRange(mapList);
                }
                db.SaveChanges();
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
                foreach (ShopTypeRequest shopTypeRq in request)
                {
                    if (shopTypeRq.ShopTypeId == null)
                    {
                        throw new Exception("Shop type id cannot be null");
                    }
                    var shopType = db.ShopTypes.Find(shopTypeRq.ShopTypeId.Value);
                    if (shopType == null)
                    {
                        throw new Exception("Cannot find user " + shopTypeRq.ShopTypeId.Value);
                    }
                    db.ShopTypes.Remove(shopType);
                }
                db.SaveChanges();
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