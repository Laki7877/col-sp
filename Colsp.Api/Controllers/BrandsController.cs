using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;

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
                
                IQueryable<Brand> brand = null;
                // List all brand
                brand = db.Brands.Where(p => true);
                brand = brand.Where(b => b.Status.Equals(Constant.STATUS_ACTIVE));

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, brand);
                }
                else
                {
                    request.DefaultOnNull();
                    if (request.SearchText != null)
                    {
                        brand = brand.Where(b => b.BrandNameEn.Contains(request.SearchText)
                        || b.BrandNameTh.Contains(request.SearchText));
                    }
                    if (request.BrandId != null)
                    {
                        brand = brand.Where(p => p.BrandId.Equals(request.BrandId));
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, brand);
                }
            }
            catch (Exception)
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

        private bool BrandExists(int id)
        {
            return db.Brands.Count(e => e.BrandId == id) > 0;
        }
    }
}