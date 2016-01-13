using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;

namespace Colsp.Api.Controllers
{
    public class BrandsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Brands")]
        [HttpGet]
        public HttpResponseMessage GetBrand([FromUri] BrandRequest request)
        {
            try
            {
                
                IQueryable<Brand> brands = null;
                // List all brand
                brands = db.Brands.Where(p => true);
                brands = brands.Where(b => b.Status.Equals(Constant.STATUS_ACTIVE));

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, brands);
                }
                else
                {
                    request.DefaultOnNull();
                    if (request.SearchText != null)
                    {
                        brands = brands.Where(b => b.BrandNameEn.Contains(request.SearchText)
                        || b.BrandNameTh.Contains(request.SearchText));
                    }
                    if (request.BrandId != null)
                    {
                        brands = brands.Where(p => p.BrandId.Equals(request.BrandId));
                    }
                    var total = brands.Count();
                    var response = PaginatedResponse.CreateResponse(brands.Paginate(request), request, total);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Brands/{brandId}")]
        [HttpGet]
        public HttpResponseMessage GetBrand(int brandId)
        {
            try
            {
                var brand = db.Brands.Find(brandId);
                if(brand != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, brand);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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

        private bool BrandExists(int id)
        {
            return db.Brands.Count(e => e.BrandId == id) > 0;
        }
    }
}