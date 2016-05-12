using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
        public HttpResponseMessage GetProductTemps([FromUri] UnGroupProductRequest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid Request");
                }
                var productTemp = db.ProductStages.Where(w => w.ProductStageGroup.GlobalCatId == request.CategoryId && (w.ProductStageGroup.AttributeSetId == null 
                || w.ProductStageGroup.AttributeSetId == request.AttributeSetId)).Select(s=>new
                {
                    s.ShopId,
                    s.Pid,
                    s.ProductNameEn,
                    s.Sku
                });
                //check if its seller permission
                if(User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    productTemp = productTemp.Where(w => w.ShopId == shopId);
                }
                else
                {
                    productTemp = productTemp.Where(w => w.ShopId == request.ShopId);
                }
                request.DefaultOnNull();

                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    productTemp = productTemp.Where(w => w.ProductNameEn.Contains(request.SearchText)
                    || w.Pid.Contains(request.SearchText)
                    || w.Sku.Contains(request.SearchText));
                }

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