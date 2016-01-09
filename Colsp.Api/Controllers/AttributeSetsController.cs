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

        private bool AttributeSetExists(int id)
        {
            return db.AttributeSets.Count(e => e.AttributeSetId == id) > 0;
        }
    }
}