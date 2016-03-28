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
    public class ProductTempsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ProductTemps")]
        [HttpGet]
        public HttpResponseMessage GetProductTemps([FromUri] ProductTempRequest request)
        {
            try
            {
                var productTemp = db.ProductTmps.Where(w => true);
                //check if its seller permission
                if (this.User.HasPermission("Group JDA"))
                {
                    //add shopid criteria for seller request
                    int shopId = this.User.ShopRequest().ShopId;
                    productTemp = productTemp.Where(w => w.Shop.ShopId == shopId);
                }
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, productTemp);
                }
                request.DefaultOnNull();
                //count number of products
                var total = productTemp.Count();
                //make paginate query from database
                var pagedProducts = productTemp.Paginate(request);
                //create response
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                //return response
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
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

        private bool ProductTempExists(long id)
        {
            return db.ProductTmps.Count(e => e.ProductId == id) > 0;
        }
    }
}