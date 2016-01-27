using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;

namespace Colsp.Api.Controllers
{
    public class CMSManagementController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/CMSShopList")]
        [HttpGet]
        public HttpResponseMessage GetByShop([FromUri] CMSShopRequest request)
        {
            try
            {
                var CMS = (from c in db.CMS
                                      where c.ShopId == request.ShopId
                                      select c
                                      ).Take(100);
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, CMS);
                }
                request.DefaultOnNull();

                //if (request.SellerId != null)
                //{
                //    collectionItem = collectionItem.Where(p => p.SellerId.Equals(request.SellerId));
                //}

                var total = CMS.Count();
                var pagedCMS = CMS.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedCMS, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
    }
}
