using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class AttributeSetsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/AttributeSets/GetFromCat")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSetFromCat(int catId)
        {
            try
            {
                var attributeSet = (from attrSet in db.AttributeSets.Include("AttributeSetMaps.Attribute")
                                    join catMap in db.CategoryAttributeSetMaps on attrSet.AttributeSetId equals catMap.AttributeSetId
                                    where catMap.CategoryId == catId
                                    select attrSet).ToList();



                if(attributeSet != null && attributeSet.Count > 0)
                {
                    return Request.CreateResponse(attributeSet);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }

            }
            catch (Exception e)
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

        private bool AttributeSetExists(int id)
        {
            return db.AttributeSets.Count(e => e.AttributeSetId == id) > 0;
        }
    }
}