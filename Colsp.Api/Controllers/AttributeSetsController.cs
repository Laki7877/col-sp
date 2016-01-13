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
using System;
using System.Data.Entity;
using System.Collections.Generic;

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
                var attrSet = db.AttributeSets.Where(w=>w.AttributeSetId.Equals(attributeSetId)).Include("AttributeSetMaps.Attribute").SingleOrDefault();
                if (attrSet != null)
                {
                    AttributeSetRequest response = new AttributeSetRequest();
                    response.AttributeSetId = attrSet.AttributeSetId;
                    response.AttributeSetNameEn = attrSet.AttributeSetNameEn;
                    response.AttributeSetNameTh = attrSet.AttributeSetNameTh;
                    response.AttributeSetDescriptionEn = attrSet.AttributeSetDescriptionEn;
                    response.AttributeSetDescriptionTh = attrSet.AttributeSetDescriptionTh;
                    if(attrSet.AttributeSetMaps != null && attrSet.AttributeSetMaps.Count > 0)
                    {
                        response.Attributes = new List<AttributeRequest>();
                        foreach (AttributeSetMap map in attrSet.AttributeSetMaps)
                        {
                            AttributeRequest attr = new AttributeRequest();
                            attr.AttributeNameEn = map.Attribute.AttributeNameEn;
                            attr.AttributeNameTh = map.Attribute.AttributeNameTh;
                            attr.AttributeUnitEn = map.Attribute.AttributeUnitEn;
                            attr.AttributeUnitTh = map.Attribute.AttributeUnitTh;
                            attr.DataType = map.Attribute.DataType;
                            attr.DataValidation = map.Attribute.DataValidation;
                            attr.DefaultValue = map.Attribute.DefaultValue;
                            attr.DisplayNameEn = map.Attribute.DisplayNameEn;
                            attr.DisplayNameTh = map.Attribute.DisplayNameTh;
                            attr.ShowAdminFlag = map.Attribute.ShowAdminFlag;
                            attr.ShowGlobalFilterFlag = map.Attribute.ShowGlobalFilterFlag;
                            attr.ShowGlobalSearchFlag = map.Attribute.ShowGlobalSearchFlag;
                            attr.ShowLocalFilterFlag = map.Attribute.ShowLocalFilterFlag;
                            attr.ShowLocalSearchFlag = map.Attribute.ShowLocalSearchFlag;
                            attr.VariantDataType = map.Attribute.VariantDataType;
                            attr.VariantStatus = map.Attribute.VariantStatus;
                            attr.AllowHtmlFlag = map.Attribute.AllowHtmlFlag;
                            response.Attributes.Add(attr);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, response);
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
        public HttpResponseMessage AddAttributeSet(AttributeSetRequest request)
        {
            AttributeSet set = null;
            try
            {
                set = new AttributeSet();
                set.AttributeSetNameEn = request.AttributeSetNameEn;
                set.AttributeSetNameTh = request.AttributeSetNameTh;
                set.AttributeSetDescriptionEn = request.AttributeSetDescriptionEn;
                set.AttributeSetDescriptionTh = request.AttributeSetDescriptionTh;
                #region Validation
                if (string.IsNullOrEmpty(set.AttributeSetNameEn))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameEn is required");
                }
                if (string.IsNullOrEmpty(set.AttributeSetNameTh))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "AttributeSetNameTh is required");
                }
                #endregion
                set.Status = Constant.STATUS_ACTIVE;
                set.CreatedBy = this.User.Email();
                set.CreatedDt = DateTime.Now;
                set = db.AttributeSets.Add(set);
                db.SaveChanges();
                if(request.Attributes != null && request.Attributes.Count > 0)
                {
                    foreach (AttributeRequest attr in request.Attributes)
                    {
                        AttributeSetMap map = new AttributeSetMap();
                        map.AttributeId = attr.AttributeId.Value;
                        map.AttributeSetId = set.AttributeSetId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        db.AttributeSetMaps.Add(map);
                    }
                }
                db.SaveChanges();
                return GetAttributeSets(set.AttributeSetId);
            }
            catch
            {
                db.Dispose();
                db = new ColspEntities();
                #region Rollback
                if (set != null)
                {
                    db.AttributeSets.Remove(set);
                    db.SaveChanges();
                }
                #endregion
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