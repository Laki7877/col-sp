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
using Colsp.Model.Requests;

namespace Colsp.Api.Controllers
{
    public class AttributesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Attributes")]
        [HttpPost]
        public HttpResponseMessage AddAttribute(Entity.Models.Attribute attribute)
        {
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(attribute.AttributeNameEn))
                { 
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameEn is required");
                }
                if (string.IsNullOrEmpty(attribute.AttributeNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameTh is required");
                }
                #endregion
                attribute = db.Attributes.Add(attribute);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, attribute);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Attributes")]
        [HttpPut]
        public HttpResponseMessage EditAttribute(Entity.Models.Attribute attribute)
        {
            try
            {
                #region Validation
                if (attribute.AttributeId.Equals(0))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Attribute is invalid");
                }
                if (string.IsNullOrEmpty(attribute.AttributeNameEn))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameEn is required");
                }
                if (string.IsNullOrEmpty(attribute.AttributeNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeNameTh is required");
                }

                var attr = db.Attributes.Find(attribute.AttributeId);
                if(attr == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                #endregion
                attr.AttributeNameEn = attribute.AttributeNameEn;
                attr.AttributeNameTh = attribute.AttributeNameTh;
                attr.AllowHtmlFlag = attribute.AllowHtmlFlag;

                attribute = db.Attributes.Add(attribute);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, attribute);
            }
            catch
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

        private bool AttributeExists(int id)
        {
            return db.Attributes.Count(e => e.AttributeId == id) > 0;
        }
    }
}