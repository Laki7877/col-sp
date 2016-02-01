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
                var shopTypeList = db.ShopTypes
                    .Select(s => new
                    {
                        s.ShopTypeId,
                        s.ShopTypeNameEn,
                        s.ShopTypeNameTh,
                        Permission = s.ShopTypePermissionMaps.Select(p=> new { p.Permission.PermissionId,p.Permission.PermissionName,p.Permission.PermissionGroup})
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
                    .Where(w => w.ShopTypeId == shopTypeid)
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