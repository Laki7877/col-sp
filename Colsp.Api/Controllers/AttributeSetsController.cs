using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using System.Data.Entity;

namespace Colsp.Api.Controllers
{
    public class AttributeSetsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/AttributeSets/GetFromAttributeSetCat/{catId}")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSetFromCat(int catId)
        {
            try
            {
                var attributeSet = (from cat in db.GlobalCategories
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
                                       where a.Status.Equals(Constant.STATUS_ACTIVE)
                                       select a,
                                attrValMap = from a in db.AttributeValueMaps
                                             where a.Status.Equals(Constant.STATUS_ACTIVE)
                                             select a,
                                attrVal = from a in db.AttributeValues
                                          where a.Status.Equals(Constant.STATUS_ACTIVE)
                                          select a
                            }).AsEnumerable().Select(t => t.attrSet).ToList();
                if (attributeSet != null && attributeSet.Count > 0)
                {
                    return Request.CreateResponse(attributeSet);
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

        private bool AttributeSetExists(int id)
        {
            return db.AttributeSets.Count(e => e.AttributeSetId == id) > 0;
        }

    }
}