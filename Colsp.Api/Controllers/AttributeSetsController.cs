using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;

namespace Colsp.Api.Controllers
{
    public class AttributeSetsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/AttributeSets/{attributSetId}/Tags")]
        [HttpGet]
        public HttpResponseMessage GetTag(int attributSetId)
        {
            try
            {
                var tag = (from attrSetTagMap in db.AttributeSetTagMaps
                          join tags in db.Tags on attrSetTagMap.TagId equals tags.TagId
                          where attrSetTagMap.AttributeSetId.Equals(attributSetId) && tags.Status.Equals(Constant.STATUS_ACTIVE) && attrSetTagMap.Status.Equals(Constant.STATUS_ACTIVE)
                          select tags).ToList();
                if(tag != null && tag.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, tag);
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

        [Route("api/AttributeSets")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSets([FromUri] AttributeSetRequest request)
        {
            try
            {
                var attrSet = from atrS in db.AttributeSets
                              select new
                              {
                                  atrS.AttributeSetId,
                                  atrS.AttributeSetNameEn,
                                  atrS.AttributeSetNameTh,
                                  atrS.UpdatedDt,
                                  atrS.CreatedDt,
                                  AttributeCount = atrS.AttributeSetMaps.AsEnumerable().Count()
                              };
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attrSet);
                }
                request.DefaultOnNull();        
                if (request.SearchText != null)
                {
                    attrSet = attrSet.Where(a => a.AttributeSetNameEn.Contains(request.SearchText)
                    || a.AttributeSetNameTh.Contains(request.SearchText));
                }
                var total = attrSet.Count();
                var response = PaginatedResponse.CreateResponse(attrSet.Paginate(request), request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/AttributeSets/{attributeSetId}")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSets(int attributeSetId)
        {
            try
            {
                var attrSet = db.AttributeSets.Find(attributeSetId);
                if (attrSet != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attrSet);
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

        [Route("api/AttributeSets")]
        [HttpPost]
        public HttpResponseMessage AddAttributeSet(AttributeSet attributeSet)
        {
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(attributeSet.AttributeSetNameEn))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameEn is required");
                }
                if (string.IsNullOrEmpty(attributeSet.AttributeSetNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameTh is required");
                }
                #endregion
                attributeSet = db.AttributeSets.Add(attributeSet);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, attributeSet);
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

        private bool AttributeSetExists(int id)
        {
            return db.AttributeSets.Count(e => e.AttributeSetId == id) > 0;
        }
    }
}