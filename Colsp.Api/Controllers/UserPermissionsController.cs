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
using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;

namespace Colsp.Api.Controllers
{
    public class UserPermissionsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        //[Route("api/UserPermissions/Admin/{PermissionRole}")]
        //[HttpGet]
        //public HttpResponseMessage GetUserPermissions(string PermissionRole)
        //{
        //    try
        //    {
        //        var userPermission = db.Permissions.Where(u => u.Type.Equals(PermissionRole));
        //        return Request.CreateResponse(HttpStatusCode.OK, userPermission);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e);
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
    }
}