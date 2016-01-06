using System.Linq;
using System.Web.Http;
using Colsp.Entity.Models;
using System.Net.Http;
using System;
using System.Net;
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class AttributesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Attributes/GetVarientFromCat")]
        [HttpGet]
        public HttpResponseMessage GetVarientAttribute(int catId)
        {
            try
            {
                var attribute = (from cat in db.GlobalCategories
                                    join catMap in db.CategoryAttributeSetMaps on cat.CategoryId equals catMap.CategoryId
                                    join attrSet in db.AttributeSets on catMap.AttributeSetId equals attrSet.AttributeSetId
                                    where cat.CategoryId.Equals(catId)
                                            && cat.Status.Equals(Constant.STATUS_ACTIVE)
                                            && attrSet.Status.Equals(Constant.STATUS_ACTIVE)
                                    select new
                                    {
                                        attrSet,
                                        attrSetMap = from a in db.AttributeSetMaps
                                                     where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                     select a,
                                        attr = from a in db.Attributes
                                               where a.Status.Equals(Constant.STATUS_ACTIVE) && a.VariantStatus.Equals(true)
                                               select a
                                    }).AsEnumerable().Select(t => t.attr).ToList();
                if (attribute != null && attribute.Count > 0)
                {
                    return Request.CreateResponse(attribute);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
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

        private bool AttributeExists(int id)
        {
            return db.Attributes.Count(e => e.AttributeId == id) > 0;
        }
    }
}