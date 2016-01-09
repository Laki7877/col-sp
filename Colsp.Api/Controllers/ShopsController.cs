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

namespace Colsp.Api.Controllers
{
    public class ShopsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Shops/{shopId}/LocalCategories")]
        [HttpGet]
        public HttpResponseMessage GetCategoryFromShop(int shopId)
        {
            try
            {
                var localCat = db.LocalCategories.Where(lc => lc.ShopId == shopId && lc.Status == Constant.STATUS_ACTIVE).ToList();
                if (localCat != null && localCat.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, localCat);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }

            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
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

        private bool ShopExists(int id)
        {
            return db.Shops.Count(e => e.ShopId == id) > 0;
        }
    }
}